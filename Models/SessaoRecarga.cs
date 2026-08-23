using MudBlazor;

namespace ev_charge_prototype.Models;

public class SessaoRecarga
{
    public string Id { get; set; } = "";
    public string CarregadorId { get; set; } = "";
    public string EstacaoNome { get; set; } = "";
    public string CarregadorCodigo { get; set; } = "";
    public string VeiculoId { get; set; } = "";
    public StatusSessao Status { get; set; } = StatusSessao.Carregando;
    public DateTime Inicio { get; set; }
    public DateTime? Fim { get; set; }
    public double BateriaInicialPercentual { get; set; }
    public double BateriaAtualPercentual { get; set; }
    public int LimiteCargaPercentual { get; set; } = 80;
    public double EnergiaAdicionadaKwh { get; set; }
    public double PotenciaAtualKw { get; set; }
    public double PotenciaMaximaKw { get; set; }
    public decimal PrecoPorKwh { get; set; }
    public decimal TaxaOcupacaoPorMinuto { get; set; }
    public decimal ValorAcumulado { get; set; }
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Pix;

    // Permanência pós-recarga: o carregador só é liberado quando o cliente confirma a retirada.
    public bool VeiculoRetirado { get; set; }
    public double PermanenciaSegundosSimulados { get; set; }
    public decimal ValorOcupacaoAcumulado { get; set; }
    public string? TransacaoId { get; set; }
}

public class Transacao
{
    public string Id { get; set; } = "";
    public DateTime Data { get; set; }
    public string EstacaoNome { get; set; } = "";
    public string CarregadorCodigo { get; set; } = "";
    public double EnergiaKwh { get; set; }
    public int DuracaoMinutos { get; set; }
    public decimal Valor { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public StatusPagamento Status { get; set; }
    public string ComprovanteId { get; set; } = "";
}

public class Notificacao
{
    public string Id { get; set; } = "";
    public DateTime DataHora { get; set; }
    public string Titulo { get; set; } = "";
    public string Mensagem { get; set; } = "";
    public string Icone { get; set; } = Icons.Material.Filled.BatteryChargingFull;
    public bool Lida { get; set; }
}

public class Plano
{
    public string Id { get; set; } = "";
    public string Nome { get; set; } = "";
    public decimal Mensalidade { get; set; }
    public List<string> Beneficios { get; set; } = new();
}

public class AcessoPortao
{
    public string Id { get; set; } = "";
    public DateTime DataHora { get; set; }
    public string UsuarioNome { get; set; } = "";
    public string Portao { get; set; } = "";
    public string Motivo { get; set; } = "";
}

public class FaturamentoDia
{
    public DateTime Data { get; set; }
    public decimal Faturamento { get; set; }
    public double KwhVendidos { get; set; }
    public int Sessoes { get; set; }
}
