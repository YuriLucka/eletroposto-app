using ev_charge_prototype.Models;

namespace ev_charge_prototype.Services;

/// <summary>Seeds and holds the prototype's static/mock dataset for the whole app session.</summary>
public class MockDataStore
{
    public Usuario UsuarioAtual { get; }
    public List<Estacao> Estacoes { get; }
    public List<Reserva> Reservas { get; }
    public List<Transacao> Transacoes { get; }
    public List<Notificacao> Notificacoes { get; }
    public List<Plano> Planos { get; }

    public MockDataStore()
    {
        UsuarioAtual = new Usuario
        {
            Id = "u1",
            Nome = "Yuri Rodrigues",
            Cpf = "000.000.000-00",
            Telefone = "(48) 99999-0000",
            Email = "yrodrigues@dpinet.com.br",
            SaldoCarteira = 42.50m,
            PlanoAtivo = null,
            PontosFidelidade = 128,
            Veiculos = new List<Veiculo>
            {
                new() { Id = "v1", Marca = "BYD", Modelo = "Dolphin", Ano = 2024, Tipo = TipoVeiculo.Eletrico,
                        Conector = TipoConector.CCS2, CapacidadeBateriaKwh = 44.9, LimiteCargaPercentual = 80, Principal = true },
                new() { Id = "v2", Marca = "Chevrolet", Modelo = "Volt", Ano = 2018, Tipo = TipoVeiculo.HibridoPlugIn,
                        Conector = TipoConector.Tipo2, CapacidadeBateriaKwh = 18.4, LimiteCargaPercentual = 90, Principal = false },
            }
        };

        Estacoes = new List<Estacao>
        {
            new()
            {
                Id = "e1", Nome = "Eletroposto Biguaçu", Endereco = "Av. Beira Mar, 1200", Cidade = "Biguaçu/SC",
                Latitude = -27.4939, Longitude = -48.6647, DistanciaMetros = 500,
                StatusPortaoEntrada = StatusPortao.Fechado, StatusPortaoSaida = StatusPortao.Fechado,
                Carregadores = new List<Carregador>
                {
                    new() { Id = "c1", EstacaoId = "e1", Codigo = "BIGUACU-DC-001", Status = StatusCarregador.Disponivel,
                            Corrente = TipoCorrente.DC, Conector = TipoConector.CCS2, PotenciaMaximaKw = 60,
                            PrecoPorKwh = 2.49m, TaxaOcupacaoPorMinuto = 0.50m, TemperaturaC = 31 },
                    new() { Id = "c2", EstacaoId = "e1", Codigo = "BIGUACU-DC-002", Status = StatusCarregador.EmUso,
                            Corrente = TipoCorrente.DC, Conector = TipoConector.CCS2, PotenciaMaximaKw = 60,
                            PrecoPorKwh = 2.49m, TaxaOcupacaoPorMinuto = 0.50m,
                            PrevisaoTerminoUso = DateTime.Now.AddMinutes(37), UsuarioAtualNome = "Carlos M.", TemperaturaC = 38 },
                    new() { Id = "c3", EstacaoId = "e1", Codigo = "BIGUACU-AC-001", Status = StatusCarregador.Disponivel,
                            Corrente = TipoCorrente.AC, Conector = TipoConector.Tipo2, PotenciaMaximaKw = 22,
                            PrecoPorKwh = 1.79m, TaxaOcupacaoPorMinuto = 0.30m, TemperaturaC = 27 },
                    new() { Id = "c4", EstacaoId = "e1", Codigo = "BIGUACU-AC-002", Status = StatusCarregador.Manutencao,
                            Corrente = TipoCorrente.AC, Conector = TipoConector.Tipo2, PotenciaMaximaKw = 22,
                            PrecoPorKwh = 1.79m, TaxaOcupacaoPorMinuto = 0.30m },
                }
            },
            new()
            {
                Id = "e2", Nome = "Eletroposto São José (em breve)", Endereco = "Av. Presidente Kennedy, 500",
                Cidade = "São José/SC", Latitude = -27.5969, Longitude = -48.6394, DistanciaMetros = 12300,
                Carregadores = new List<Carregador>
                {
                    new() { Id = "c5", EstacaoId = "e2", Codigo = "SJ-DC-001", Status = StatusCarregador.Indisponivel,
                            Corrente = TipoCorrente.DC, Conector = TipoConector.CCS2, PotenciaMaximaKw = 60,
                            PrecoPorKwh = 2.59m, TaxaOcupacaoPorMinuto = 0.50m },
                }
            }
        };

        Reservas = new List<Reserva>
        {
            new() { Id = "r1", CarregadorId = "c3", EstacaoNome = "Eletroposto Biguaçu", CarregadorCodigo = "BIGUACU-AC-001",
                    DataHora = DateTime.Today.AddHours(18), DuracaoMinutos = 30, Status = StatusReserva.Confirmada,
                    CodigoQr = "RSV-8F2A91" }
        };

        Transacoes = new List<Transacao>
        {
            new() { Id = "t1", Data = DateTime.Today.AddDays(-2).AddHours(19).AddMinutes(10), EstacaoNome = "Eletroposto Biguaçu",
                    CarregadorCodigo = "BIGUACU-DC-001", EnergiaKwh = 32.5, DuracaoMinutos = 42, Valor = 80.93m,
                    FormaPagamento = FormaPagamento.Pix, Status = StatusPagamento.Aprovado, ComprovanteId = "CMP-0001" },
            new() { Id = "t2", Data = DateTime.Today.AddDays(-7).AddHours(8).AddMinutes(5), EstacaoNome = "Eletroposto Biguaçu",
                    CarregadorCodigo = "BIGUACU-AC-001", EnergiaKwh = 18.2, DuracaoMinutos = 65, Valor = 32.58m,
                    FormaPagamento = FormaPagamento.CartaoCredito, Status = StatusPagamento.Aprovado, ComprovanteId = "CMP-0002" },
            new() { Id = "t3", Data = DateTime.Today.AddDays(-15).AddHours(21).AddMinutes(40), EstacaoNome = "Eletroposto Biguaçu",
                    CarregadorCodigo = "BIGUACU-DC-001", EnergiaKwh = 40.1, DuracaoMinutos = 51, Valor = 99.85m,
                    FormaPagamento = FormaPagamento.CarteiraPrepaga, Status = StatusPagamento.Aprovado, ComprovanteId = "CMP-0003" },
        };

        Notificacoes = new List<Notificacao>
        {
            new() { Id = "n1", DataHora = DateTime.Now.AddMinutes(-5), Titulo = "Reserva confirmada",
                    Mensagem = "Sua reserva no carregador BIGUACU-AC-001 às 18:00 foi confirmada.", Icone = "📅" },
            new() { Id = "n2", DataHora = DateTime.Now.AddHours(-3), Titulo = "Pagamento aprovado",
                    Mensagem = "Pagamento de R$ 80,93 aprovado via Pix.", Icone = "✅", Lida = true },
            new() { Id = "n3", DataHora = DateTime.Now.AddDays(-2), Titulo = "Recarga finalizada",
                    Mensagem = "Seu veículo atingiu 80%. Retire o veículo para evitar cobrança de permanência.", Icone = "🔋", Lida = true },
        };

        Planos = new List<Plano>
        {
            new() { Id = "p1", Nome = "Plano Morador", Mensalidade = 49.90m,
                    Beneficios = new() { "Preço diferenciado no kWh", "Prioridade de reserva", "Desconto no kWh", "Histórico completo", "Carregamento recorrente" } },
            new() { Id = "p2", Nome = "Plano Empresarial (em breve)", Mensalidade = 0,
                    Beneficios = new() { "Faturamento centralizado para frotas", "Múltiplos veículos", "Relatórios de consumo" } },
        };
    }
}
