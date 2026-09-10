namespace Refeicao.Core.Abstractions.Authentication;

public interface ITokenService
{
    (string Token, DateTime ExpiraEm) GerarToken(string id, string nome, string papel);
}
