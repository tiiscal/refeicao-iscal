namespace Refeicao.Api.Contracts;

public record LoginRequest(string Id, string Senha);

public record LoginResponse(string Token, DateTime ExpiraEm, string Papel, bool PrimeiroAcesso);

public record AlterarSenhaRequest(string SenhaAtual, string NovaSenha);
