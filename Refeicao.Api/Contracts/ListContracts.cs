namespace Refeicao.Api.Contracts;

public record ListarCadastrosResponse(IReadOnlyList<UsuarioListItem> Usuarios, IReadOnlyList<FuncionarioListItem> Funcionarios);
