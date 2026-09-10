using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Refeicao.Core.Services;

namespace Refeicao.Tests.Services;

public class TokenServiceTests
{
    private const string ChaveValida = "chave-de-teste-com-tamanho-suficiente-para-hmac-sha256";

    private static TokenService CriarTokenService(JwtOptions? opcoes = null)
    {
        opcoes ??= new JwtOptions
        {
            Key = ChaveValida,
            Issuer = "RefeicaoIscalTeste",
            Audience = "RefeicaoIscalTeste",
            ExpiresInMinutes = 30,
        };

        return new TokenService(Options.Create(opcoes));
    }

    [Fact]
    public void GerarToken_DeveRetornarTokenNaoVazio()
    {
        var tokenService = CriarTokenService();

        var (token, _) = tokenService.GerarToken("user01", "Samuel Silva", "Usuario");

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GerarToken_DeveCalcularExpiracaoConformeConfiguracao()
    {
        var opcoes = new JwtOptions
        {
            Key = ChaveValida, Issuer = "Iss", Audience = "Aud", ExpiresInMinutes = 45,
        };
        var tokenService = CriarTokenService(opcoes);

        var antes = DateTime.UtcNow;
        var (_, expiraEm) = tokenService.GerarToken("user01", "Samuel Silva", "Usuario");
        var depois = DateTime.UtcNow;

        Assert.InRange(expiraEm, antes.AddMinutes(45), depois.AddMinutes(45));
    }

    [Fact]
    public void GerarToken_DeveIncluirClaimsCorretas()
    {
        var tokenService = CriarTokenService();

        var (token, _) = tokenService.GerarToken("user01", "Samuel Silva", "Usuario");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("user01", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("user01", jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("Samuel Silva", jwt.Claims.Single(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal("Usuario", jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void GerarToken_DeveGerarJtiUnicoACadaChamada()
    {
        var tokenService = CriarTokenService();
        var handler = new JwtSecurityTokenHandler();

        var (token1, _) = tokenService.GerarToken("user01", "Samuel Silva", "Usuario");
        var (token2, _) = tokenService.GerarToken("user01", "Samuel Silva", "Usuario");

        var jti1 = handler.ReadJwtToken(token1).Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = handler.ReadJwtToken(token2).Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        Assert.NotEqual(jti1, jti2);
    }

    [Fact]
    public void GerarToken_DeveDefinirIssuerEAudienceConformeConfiguracao()
    {
        var opcoes = new JwtOptions
        {
            Key = ChaveValida, Issuer = "MeuIssuer", Audience = "MinhaAudience", ExpiresInMinutes = 60,
        };
        var tokenService = CriarTokenService(opcoes);

        var (token, _) = tokenService.GerarToken("user01", "Samuel Silva", "Usuario");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("MeuIssuer", jwt.Issuer);
        Assert.Contains("MinhaAudience", jwt.Audiences);
    }

    [Fact]
    public void GerarToken_DeveGerarAssinaturaValidaComAChaveConfigurada()
    {
        var opcoes = new JwtOptions { Key = ChaveValida, Issuer = "Iss", Audience = "Aud", ExpiresInMinutes = 60 };
        var tokenService = CriarTokenService(opcoes);
        var (token, _) = tokenService.GerarToken("user01", "Samuel Silva", "Usuario");

        var parametros = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "Iss",
            ValidateAudience = true,
            ValidAudience = "Aud",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ChaveValida)),
            ClockSkew = TimeSpan.Zero,
        };

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, parametros, out _);

        Assert.True(principal.Identity?.IsAuthenticated);
    }

    [Fact]
    public void GerarToken_ComChaveDeValidacaoDiferente_DeveFalharNaValidacao()
    {
        var tokenService = CriarTokenService();
        var (token, _) = tokenService.GerarToken("user01", "Samuel Silva", "Usuario");

        var parametros = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("outra-chave-completamente-diferente-e-invalida")),
        };

        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(
            () => new JwtSecurityTokenHandler().ValidateToken(token, parametros, out _));
    }
}
