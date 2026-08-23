using MudBlazor;
using ev_charge_prototype.Models;

namespace ev_charge_prototype.Pages.Cliente;

/// <summary>
/// Small formatting/icon-mapping helpers shared across the Cliente (end-customer) pages.
/// Pure presentation helpers only — no state, no side effects, safe to keep local to this folder.
/// </summary>
public static class Format
{
    public static string Moeda(decimal v) => $"R$ {v:N2}";
    public static string Data(DateTime d) => d.ToString("dd/MM/yyyy");
    public static string Hora(DateTime d) => d.ToString("HH:mm");
    public static string DataHora(DateTime d) => d.ToString("dd/MM/yyyy HH:mm");
    public static string Distancia(double metros) => metros >= 1000 ? $"{metros / 1000:0.#} km" : $"{metros:0} m";

    public static string PortaoTexto(StatusPortao s) => s switch
    {
        StatusPortao.Fechado => "Fechado",
        StatusPortao.Abrindo => "Abrindo...",
        StatusPortao.Aberto => "Aberto",
        StatusPortao.Falha => "Falha",
        _ => s.ToString(),
    };

    public static Color PortaoCor(StatusPortao s) => s switch
    {
        StatusPortao.Fechado => Color.Default,
        StatusPortao.Abrindo => Color.Warning,
        StatusPortao.Aberto => Color.Success,
        StatusPortao.Falha => Color.Error,
        _ => Color.Default,
    };

    public static string PortaoIcone(StatusPortao s) => s switch
    {
        StatusPortao.Fechado => Icons.Material.Filled.Lock,
        StatusPortao.Abrindo => Icons.Material.Filled.Sync,
        StatusPortao.Aberto => Icons.Material.Filled.LockOpen,
        StatusPortao.Falha => Icons.Material.Filled.ErrorOutline,
        _ => Icons.Material.Filled.HelpOutline,
    };

    public static string FormaPagamentoIcone(FormaPagamento f) => f switch
    {
        FormaPagamento.Pix => Icons.Material.Filled.QrCode2,
        FormaPagamento.CartaoCredito => Icons.Material.Filled.CreditCard,
        FormaPagamento.CartaoDebito => Icons.Material.Filled.CreditCard,
        FormaPagamento.CarteiraPrepaga => Icons.Material.Filled.AccountBalanceWallet,
        FormaPagamento.PlanoMensal => Icons.Material.Filled.CardMembership,
        _ => Icons.Material.Filled.Payments,
    };

    public static string VeiculoIcone(TipoVeiculo t) => t switch
    {
        TipoVeiculo.Eletrico => Icons.Material.Filled.ElectricCar,
        TipoVeiculo.HibridoPlugIn => Icons.Material.Filled.EvStation,
        _ => Icons.Material.Filled.DirectionsCar,
    };

    public static string ConectorIcone(TipoConector c) => Icons.Material.Filled.Cable;

    public static string StatusReservaTexto(StatusReserva s) => s switch
    {
        StatusReserva.Confirmada => "Confirmada",
        StatusReserva.EmAndamento => "Em andamento",
        StatusReserva.Concluida => "Concluída",
        StatusReserva.Cancelada => "Cancelada",
        StatusReserva.NoShow => "No-show",
        _ => s.ToString(),
    };

    public static Color StatusReservaCor(StatusReserva s) => s switch
    {
        StatusReserva.Confirmada => Color.Primary,
        StatusReserva.EmAndamento => Color.Info,
        StatusReserva.Concluida => Color.Success,
        StatusReserva.Cancelada => Color.Default,
        StatusReserva.NoShow => Color.Error,
        _ => Color.Default,
    };
}
