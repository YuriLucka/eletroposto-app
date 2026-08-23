namespace ev_charge_prototype.Models;

public class Veiculo
{
    public string Id { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public int Ano { get; set; }
    public TipoVeiculo Tipo { get; set; } = TipoVeiculo.Eletrico;
    public TipoConector Conector { get; set; } = TipoConector.CCS2;
    public double CapacidadeBateriaKwh { get; set; }
    public int LimiteCargaPercentual { get; set; } = 80;
    public bool Principal { get; set; }
}

public class Usuario
{
    public string Id { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Cpf { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal SaldoCarteira { get; set; }
    public string? PlanoAtivo { get; set; }
    public int PontosFidelidade { get; set; }
    public List<Veiculo> Veiculos { get; set; } = new();
    public NivelAcesso? NivelAdmin { get; set; }
}
