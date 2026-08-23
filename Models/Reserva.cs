namespace ev_charge_prototype.Models;

public class Reserva
{
    public string Id { get; set; } = "";
    public string CarregadorId { get; set; } = "";
    public string EstacaoNome { get; set; } = "";
    public string CarregadorCodigo { get; set; } = "";
    public DateTime DataHora { get; set; }
    public int DuracaoMinutos { get; set; }
    public StatusReserva Status { get; set; } = StatusReserva.Confirmada;
    public string CodigoQr { get; set; } = "";
}
