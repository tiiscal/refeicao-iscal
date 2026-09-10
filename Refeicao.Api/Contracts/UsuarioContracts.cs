namespace Refeicao.Api.Contracts;

public record CadastroUsuarioRequest(string Chave, string IdUsuario, string NmUsuario, string Senha);

public record CadastroUsuarioResponse(string IdUsuario, string NmUsuario);

public record UsuarioListItem(
    string IdUsuario, string NmUsuario, bool PrimeiroAcesso, DateTime DtCadastro, DateTime? DtInativacao);

public record UsuarioListResponse(string IdUsuario, string NmUsuario, DateTime DtCadastro);
