namespace ev_charge_prototype.Models;

public class Estacao
{
    public string Id { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Endereco { get; set; } = "";
    public string Cidade { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanciaMetros { get; set; }
    public List<Carregador> Carregadores { get; set; } = new();
    public StatusPortao StatusPortaoEntrada { get; set; } = StatusPortao.Fechado;
    public StatusPortao StatusPortaoSaida { get; set; } = StatusPortao.Fechado;
    public DateTime? PortaoAbertoDesde { get; set; }

    public int Disponiveis => Carregadores.Count(c => c.Status == StatusCarregador.Disponivel);
    public int Total => Carregadores.Count;
}

public class Carregador
{
    public string Id { get; set; } = "";
    public string EstacaoId { get; set; } = "";
    public string Codigo { get; set; } = "";
    public StatusCarregador Status { get; set; } = StatusCarregador.Disponivel;
    public TipoCorrente Corrente { get; set; } = TipoCorrente.DC;
    public TipoConector Conector { get; set; } = TipoConector.CCS2;
    public double PotenciaMaximaKw { get; set; }
    public decimal PrecoPorKwh { get; set; }
    public decimal TaxaOcupacaoPorMinuto { get; set; }
    public DateTime? PrevisaoTerminoUso { get; set; }
    public string? UsuarioAtualNome { get; set; }
    public string? SessaoAtualId { get; set; }
    public double? TemperaturaC { get; set; }
}
