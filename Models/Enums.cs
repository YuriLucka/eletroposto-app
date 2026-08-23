namespace ev_charge_prototype.Models;

public enum StatusCarregador { Disponivel, EmUso, Manutencao, Indisponivel }

public enum TipoCorrente { AC, DC }

public enum TipoConector { CCS2, Tipo2, CHAdeMO }

public enum StatusReserva { Confirmada, EmAndamento, Concluida, Cancelada, NoShow }

public enum StatusSessao { AguardandoConexao, Carregando, Finalizada, Cancelada }

public enum StatusPagamento { Aprovado, Recusado, Pendente }

public enum FormaPagamento { Pix, CartaoCredito, CartaoDebito, CarteiraPrepaga, PlanoMensal }

public enum StatusPortao { Fechado, Abrindo, Aberto, Falha }

public enum NivelAcesso { Proprietario, Operador, Financeiro, Tecnico }

public enum TipoVeiculo { Eletrico, HibridoPlugIn }
