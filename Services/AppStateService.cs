using MudBlazor;
using ev_charge_prototype.Models;

namespace ev_charge_prototype.Services;

/// <summary>
/// Single in-memory source of truth for the prototype: active charging session, gate status and
/// the tick-based simulation that stands in for real OCPP telemetry. No persistence — refreshing
/// the browser resets state, which is acceptable for a static-data demo.
/// </summary>
public class AppStateService : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private readonly Random _rng = new();

    public MockDataStore Data { get; }
    public SessaoRecarga? SessaoAtiva { get; private set; }
    public event Action? OnChange;

    public AppStateService(MockDataStore data)
    {
        Data = data;
        _timer = new System.Timers.Timer(1500);
        _timer.Elapsed += (_, _) => Tick();
        _timer.Start();
    }

    private Carregador? CarregadorDaSessao() =>
        SessaoAtiva is null ? null : Data.Estacoes.SelectMany(e => e.Carregadores).FirstOrDefault(c => c.Id == SessaoAtiva.CarregadorId);

    private void Tick()
    {
        var mudou = false;

        if (SessaoAtiva is { Status: StatusSessao.Carregando } sessao)
        {
            var carregador = CarregadorDaSessao();
            var potenciaMax = sessao.PotenciaMaximaKw;
            // potência oscila perto do máximo, reduz conforme aproxima do limite (curva de carga realista)
            var margem = Math.Max(0, sessao.LimiteCargaPercentual - sessao.BateriaAtualPercentual);
            var fator = margem < 15 ? 0.4 : 1.0;
            sessao.PotenciaAtualKw = Math.Round(potenciaMax * fator * (0.85 + _rng.NextDouble() * 0.15), 1);

            var kwhIncremento = sessao.PotenciaAtualKw * (_timer.Interval / 3_600_000.0) * 6; // acelera a demo
            sessao.EnergiaAdicionadaKwh = Math.Round(sessao.EnergiaAdicionadaKwh + kwhIncremento, 2);

            var capacidade = Data.UsuarioAtual.Veiculos.FirstOrDefault(v => v.Id == sessao.VeiculoId)?.CapacidadeBateriaKwh ?? 45;
            var percentualGanho = (kwhIncremento / capacidade) * 100;
            sessao.BateriaAtualPercentual = Math.Min(100, Math.Round(sessao.BateriaAtualPercentual + percentualGanho, 1));

            sessao.ValorAcumulado = Math.Round((decimal)sessao.EnergiaAdicionadaKwh * sessao.PrecoPorKwh, 2);

            if (carregador is not null)
                carregador.TemperaturaC = Math.Round(30 + sessao.PotenciaAtualKw / 3.0 + _rng.NextDouble() * 2, 1);

            if (sessao.BateriaAtualPercentual >= sessao.LimiteCargaPercentual && !_limiteNotificado)
            {
                _limiteNotificado = true;
                AddNotificacao(Icons.Material.Filled.BatteryChargingFull, "Limite de bateria atingido",
                    $"Seu veículo atingiu {sessao.LimiteCargaPercentual}%. Você já pode encerrar a recarga.");
            }

            mudou = true;
        }
        else if (SessaoAtiva is { Status: StatusSessao.Finalizada, VeiculoRetirado: false } encerrada)
        {
            const double AceleracaoDemo = 6.0; // mesmo fator usado na simulação de carga
            const double ToleranciaSegundos = 6.0; // poucos segundos de tolerância antes de avisar/cobrar

            encerrada.PermanenciaSegundosSimulados += (_timer.Interval / 1000.0) * AceleracaoDemo;

            if (encerrada.PermanenciaSegundosSimulados > ToleranciaSegundos)
            {
                var minutosCobraveis = (encerrada.PermanenciaSegundosSimulados - ToleranciaSegundos) / 60.0;
                encerrada.ValorOcupacaoAcumulado = Math.Round((decimal)minutosCobraveis * encerrada.TaxaOcupacaoPorMinuto, 2);

                if (!_permanenciaAvisada)
                {
                    _permanenciaAvisada = true;
                    AddNotificacao(Icons.Material.Filled.DirectionsCarFilled, "Veículo permanece conectado",
                        $"Retire seu veículo do carregador {encerrada.CarregadorCodigo} para evitar cobrança de permanência.");
                }
                else if (!_taxaOcupacaoAvisada)
                {
                    _taxaOcupacaoAvisada = true;
                    AddNotificacao(Icons.Material.Filled.LocalParking, "Taxa de ocupação iniciada",
                        $"{Format2(encerrada.TaxaOcupacaoPorMinuto)}/min está sendo cobrado enquanto o veículo permanecer conectado.");
                }
            }

            mudou = true;
        }

        if (mudou) OnChange?.Invoke();
    }

    private static string Format2(decimal v) => $"R$ {v:N2}";

    private bool _limiteNotificado;
    private bool _permanenciaAvisada;
    private bool _taxaOcupacaoAvisada;

    /// <summary>True enquanto houver uma sessão carregando ou aguardando confirmação de retirada — o protótipo simula apenas um veículo/sessão por vez.</summary>
    public bool TemSessaoPendente => SessaoAtiva is not null;

    public SessaoRecarga IniciarRecarga(string carregadorId, string veiculoId, FormaPagamento formaPagamento)
    {
        if (SessaoAtiva is not null)
            throw new InvalidOperationException("Já existe uma recarga em andamento ou aguardando retirada do veículo.");

        var carregador = Data.Estacoes.SelectMany(e => e.Carregadores).First(c => c.Id == carregadorId);
        var estacao = Data.Estacoes.First(e => e.Carregadores.Contains(carregador));
        var veiculo = Data.UsuarioAtual.Veiculos.First(v => v.Id == veiculoId);

        carregador.Status = StatusCarregador.EmUso;
        carregador.UsuarioAtualNome = Data.UsuarioAtual.Nome;

        _limiteNotificado = false;
        _permanenciaAvisada = false;
        _taxaOcupacaoAvisada = false;
        SessaoAtiva = new SessaoRecarga
        {
            Id = $"s-{Guid.NewGuid().ToString()[..8]}",
            CarregadorId = carregador.Id,
            EstacaoNome = estacao.Nome,
            CarregadorCodigo = carregador.Codigo,
            VeiculoId = veiculo.Id,
            Status = StatusSessao.Carregando,
            Inicio = DateTime.Now,
            BateriaInicialPercentual = 32,
            BateriaAtualPercentual = 32,
            LimiteCargaPercentual = veiculo.LimiteCargaPercentual,
            PotenciaMaximaKw = carregador.PotenciaMaximaKw,
            PrecoPorKwh = carregador.PrecoPorKwh,
            TaxaOcupacaoPorMinuto = carregador.TaxaOcupacaoPorMinuto,
            FormaPagamento = formaPagamento,
        };

        carregador.SessaoAtualId = SessaoAtiva.Id;
        AddNotificacao(Icons.Material.Filled.ElectricBolt, "Recarga iniciada", $"Carregador {carregador.Codigo} — {estacao.Nome}.");
        OnChange?.Invoke();
        return SessaoAtiva;
    }

    public Transacao FinalizarRecarga()
    {
        if (SessaoAtiva is null) throw new InvalidOperationException("Nenhuma recarga ativa.");

        var sessao = SessaoAtiva;
        sessao.Status = StatusSessao.Finalizada;
        sessao.Fim = DateTime.Now;
        // O carregador só é liberado quando o cliente confirma a retirada do veículo
        // (ver ConfirmarRetiradaVeiculo) — até lá, permanece ocupado e pode gerar taxa de permanência.

        var transacao = new Transacao
        {
            Id = $"t-{Guid.NewGuid().ToString()[..8]}",
            Data = sessao.Fim.Value,
            EstacaoNome = sessao.EstacaoNome,
            CarregadorCodigo = sessao.CarregadorCodigo,
            EnergiaKwh = sessao.EnergiaAdicionadaKwh,
            DuracaoMinutos = (int)Math.Ceiling((sessao.Fim.Value - sessao.Inicio).TotalMinutes),
            Valor = sessao.ValorAcumulado,
            FormaPagamento = sessao.FormaPagamento,
            Status = StatusPagamento.Aprovado,
            ComprovanteId = $"CMP-{_rng.Next(1000, 9999)}",
        };
        Data.Transacoes.Insert(0, transacao);
        Data.UsuarioAtual.PontosFidelidade += (int)transacao.Valor;
        sessao.TransacaoId = transacao.Id;

        AddNotificacao(Icons.Material.Filled.CheckCircle, "Pagamento aprovado", $"Recarga finalizada — R$ {transacao.Valor:N2} via {DescreverFormaPagamento(transacao.FormaPagamento)}.");

        OnChange?.Invoke();
        return transacao;
    }

    public void ConfirmarRetiradaVeiculo()
    {
        if (SessaoAtiva is not { Status: StatusSessao.Finalizada } sessao) return;

        var carregador = CarregadorDaSessao();
        if (carregador is not null)
        {
            carregador.UsuarioAtualNome = null;
            carregador.SessaoAtualId = null;
            carregador.PrevisaoTerminoUso = null;
            // Preserva uma manutenção sinalizada pelo admin enquanto o veículo ainda estava conectado
            // — só volta a Disponível se ninguém colocou o carregador fora de operação nesse meio-tempo.
            if (carregador.Status is not (StatusCarregador.Manutencao or StatusCarregador.Indisponivel))
                carregador.Status = StatusCarregador.Disponivel;
        }

        sessao.VeiculoRetirado = true;
        if (sessao.ValorOcupacaoAcumulado > 0)
        {
            var cobranca = new Transacao
            {
                Id = $"t-{Guid.NewGuid().ToString()[..8]}",
                Data = DateTime.Now,
                EstacaoNome = sessao.EstacaoNome,
                CarregadorCodigo = sessao.CarregadorCodigo,
                EnergiaKwh = 0,
                DuracaoMinutos = (int)Math.Ceiling(sessao.PermanenciaSegundosSimulados / 60.0),
                Valor = sessao.ValorOcupacaoAcumulado,
                FormaPagamento = sessao.FormaPagamento,
                Status = StatusPagamento.Aprovado,
                ComprovanteId = $"CMP-{_rng.Next(1000, 9999)}",
            };
            Data.Transacoes.Insert(0, cobranca);

            AddNotificacao(Icons.Material.Filled.CheckCircle, "Veículo retirado",
                $"Cobrança de permanência: {Format2(sessao.ValorOcupacaoAcumulado)}.");
        }

        SessaoAtiva = null;
        OnChange?.Invoke();
    }

    public static string DescreverFormaPagamento(FormaPagamento f) => f switch
    {
        FormaPagamento.Pix => "Pix",
        FormaPagamento.CartaoCredito => "Cartão de crédito",
        FormaPagamento.CartaoDebito => "Cartão de débito",
        FormaPagamento.CarteiraPrepaga => "Carteira pré-paga",
        FormaPagamento.PlanoMensal => "Plano mensal",
        _ => f.ToString(),
    };

    /// <summary>Retorna false sem abrir nada quando o acesso está bloqueado pelo admin — chame antes de exibir o botão de abrir como se fosse funcionar.</summary>
    public async Task<bool> AbrirPortaoAsync(Estacao estacao, bool entrada = true, string motivo = "Abertura pelo aplicativo")
    {
        if (estacao.AcessoBloqueado)
        {
            AddNotificacao(Icons.Material.Filled.Block, "Acesso bloqueado",
                $"O acesso a {estacao.Nome} está bloqueado no momento. Fale com o suporte.");
            return false;
        }

        void SetStatus(StatusPortao s)
        {
            if (entrada) estacao.StatusPortaoEntrada = s; else estacao.StatusPortaoSaida = s;
            OnChange?.Invoke();
        }

        SetStatus(StatusPortao.Abrindo);
        await Task.Delay(1200);
        SetStatus(StatusPortao.Aberto);
        if (entrada) estacao.PortaoEntradaAbertoDesde = DateTime.Now; else estacao.PortaoSaidaAbertoDesde = DateTime.Now;

        Data.AcessosPortao.Insert(0, new AcessoPortao
        {
            Id = $"ap-{Guid.NewGuid().ToString()[..8]}",
            DataHora = DateTime.Now,
            UsuarioNome = Data.UsuarioAtual.Nome,
            Portao = entrada ? "Entrada" : "Saída",
            Motivo = motivo,
        });

        AddNotificacao(Icons.Material.Filled.SensorDoor, entrada ? "Portão de entrada aberto" : "Portão de saída aberto", estacao.Nome);
        return true;
    }

    public void FecharPortao(Estacao estacao, bool entrada = true)
    {
        if (entrada) { estacao.StatusPortaoEntrada = StatusPortao.Fechado; estacao.PortaoEntradaAbertoDesde = null; }
        else { estacao.StatusPortaoSaida = StatusPortao.Fechado; estacao.PortaoSaidaAbertoDesde = null; }
        OnChange?.Invoke();
    }

    public void AddNotificacao(string icone, string titulo, string mensagem)
    {
        Data.Notificacoes.Insert(0, new Notificacao
        {
            Id = $"n-{Guid.NewGuid().ToString()[..8]}",
            DataHora = DateTime.Now,
            Titulo = titulo,
            Mensagem = mensagem,
            Icone = icone,
        });
        OnChange?.Invoke();
    }

    public void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose() => _timer.Dispose();
}
