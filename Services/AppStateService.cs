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

        if (mudou) OnChange?.Invoke();
    }

    private bool _limiteNotificado;

    public SessaoRecarga IniciarRecarga(string carregadorId, string veiculoId, FormaPagamento formaPagamento)
    {
        var carregador = Data.Estacoes.SelectMany(e => e.Carregadores).First(c => c.Id == carregadorId);
        var estacao = Data.Estacoes.First(e => e.Carregadores.Contains(carregador));
        var veiculo = Data.UsuarioAtual.Veiculos.First(v => v.Id == veiculoId);

        carregador.Status = StatusCarregador.EmUso;
        carregador.UsuarioAtualNome = Data.UsuarioAtual.Nome;

        _limiteNotificado = false;
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

        var carregador = CarregadorDaSessao();
        if (carregador is not null)
        {
            carregador.Status = StatusCarregador.Disponivel;
            carregador.UsuarioAtualNome = null;
            carregador.SessaoAtualId = null;
            carregador.PrevisaoTerminoUso = null;
        }

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

        AddNotificacao(Icons.Material.Filled.CheckCircle, "Pagamento aprovado", $"Recarga finalizada — R$ {transacao.Valor:N2} via {DescreverFormaPagamento(transacao.FormaPagamento)}.");

        SessaoAtiva = null;
        OnChange?.Invoke();
        return transacao;
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

    public async Task AbrirPortaoAsync(Estacao estacao, bool entrada = true)
    {
        void SetStatus(StatusPortao s)
        {
            if (entrada) estacao.StatusPortaoEntrada = s; else estacao.StatusPortaoSaida = s;
            OnChange?.Invoke();
        }

        SetStatus(StatusPortao.Abrindo);
        await Task.Delay(1200);
        SetStatus(StatusPortao.Aberto);
        estacao.PortaoAbertoDesde = DateTime.Now;
        AddNotificacao(Icons.Material.Filled.SensorDoor, entrada ? "Portão de entrada aberto" : "Portão de saída aberto", estacao.Nome);
    }

    public void FecharPortao(Estacao estacao, bool entrada = true)
    {
        if (entrada) estacao.StatusPortaoEntrada = StatusPortao.Fechado; else estacao.StatusPortaoSaida = StatusPortao.Fechado;
        estacao.PortaoAbertoDesde = null;
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
