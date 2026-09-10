namespace Refeicao.Api.Contracts;

public record CadastroFuncionarioRequest(string Chave, string NmUsuario, string Senha);

public record CadastroFuncionarioResponse(int IdFuncionario, string NmUsuario);

public record FuncionarioListItem(int IdFuncionario, string NmUsuario, bool PrimeiroAcesso, DateTime DtCadastro, DateTime? DtInativacao);

public record FuncionarioListResponse(int IdFuncionario, string NmUsuario, DateTime DtCadastro);
