using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Refeicao.Api.Authentication;
using Refeicao.Api.Config;
using Refeicao.Api.Contracts;
using Refeicao.Core.Abstractions.Authentication;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/auth")
            .WithTags("Autenticação")
            .RequireRateLimiting(BuilderConfiguration.AuthRateLimiterPolicyName);

        grupo.MapPost("/usuario/login", async (
            LoginRequest requisicao,
            IUsuarioRepository repositorio,
            IPasswordHasher<Usuario> hasher,
            ITokenService tokenService) =>
        {
            var usuario = await repositorio.GetByIdAsync(requisicao.Id);
            if (usuario is null || usuario.DtInativacao is not null)
                return Results.Unauthorized();

            if (hasher.VerifyHashedPassword(usuario, usuario.HashSenha, requisicao.Senha) ==
                PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            var (token, expiraEm) = tokenService.GerarToken(usuario.IdUsuario, usuario.NmUsuario, Papeis.Usuario);
            return Results.Ok(new LoginResponse(token, expiraEm, Papeis.Usuario, usuario.PrimeiroAcesso));
        })
            .WithSummary("Login de usuário")
            .WithDescription(
                "Autentica um Usuario pelo IdUsuario e senha, retornando um token JWT e se é o primeiro acesso " +
                "(primeiroAcesso=true exige trocar a senha antes de usar qualquer outro endpoint). " +
                "Retorna 401 se as credenciais forem inválidas ou a conta estiver inativa.");

        grupo.MapPost("/funcionario/login", async (
            LoginRequest requisicao,
            IFuncionarioRepository repositorio,
            IPasswordHasher<Funcionario> hasher,
            ITokenService tokenService) =>
        {
            var funcionario = await repositorio.GetByUsernameAsync(requisicao.Id);
            if (funcionario is null || funcionario.DtInativacao is not null)
                return Results.Unauthorized();

            if (hasher.VerifyHashedPassword(funcionario, funcionario.HashSenha, requisicao.Senha) ==
                PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            var (token, expiraEm) = tokenService.GerarToken(
                funcionario.IdFuncionario.ToString(), funcionario.NmUsuario, Papeis.Funcionario);
            return Results.Ok(new LoginResponse(token, expiraEm, Papeis.Funcionario, funcionario.PrimeiroAcesso));
        })
            .WithSummary("Login de funcionário")
            .WithDescription(
                "Autentica um Funcionario pelo nome de usuário e senha, retornando um token JWT e se é o primeiro " +
                "acesso (primeiroAcesso=true exige trocar a senha antes de usar qualquer outro endpoint). " +
                "Retorna 401 se as credenciais forem inválidas ou a conta estiver inativa.");

        grupo.MapPost("/usuario/alterar-senha", async (
            AlterarSenhaRequest requisicao,
            ClaimsPrincipal usuarioLogado,
            IUsuarioRepository repositorio,
            IPasswordHasher<Usuario> hasher) =>
        {
            if (string.IsNullOrWhiteSpace(requisicao.NovaSenha))
                return Results.BadRequest();

            var id = usuarioLogado.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var usuario = await repositorio.GetByIdAsync(id);
            if (usuario is null || usuario.DtInativacao is not null)
                return Results.Unauthorized();

            if (hasher.VerifyHashedPassword(usuario, usuario.HashSenha, requisicao.SenhaAtual) ==
                PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            if (requisicao.NovaSenha == requisicao.SenhaAtual)
                return Results.BadRequest();

            usuario.HashSenha = hasher.HashPassword(usuario, requisicao.NovaSenha);
            usuario.PrimeiroAcesso = false;
            repositorio.UpdateAsync(usuario);
            await repositorio.SaveChangesAsync();

            return Results.NoContent();
        })
            .WithSummary("Alterar senha do usuário")
            .WithDescription(
                "Permite que um Usuario autenticado troque a própria senha, informando a senha atual e a nova. " +
                "É o único endpoint acessível enquanto o primeiro acesso ainda não foi concluído.")
            .RequireAuthorization(policy => policy.RequireRole(Papeis.Usuario));

        grupo.MapPost("/funcionario/alterar-senha", async (
            AlterarSenhaRequest requisicao,
            ClaimsPrincipal usuarioLogado,
            IFuncionarioRepository repositorio,
            IPasswordHasher<Funcionario> hasher) =>
        {
            if (string.IsNullOrWhiteSpace(requisicao.NovaSenha))
                return Results.BadRequest();

            var id = int.Parse(usuarioLogado.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var funcionario = await repositorio.GetByIdAsync(id);
            if (funcionario is null || funcionario.DtInativacao is not null)
                return Results.Unauthorized();

            if (hasher.VerifyHashedPassword(funcionario, funcionario.HashSenha, requisicao.SenhaAtual) ==
                PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            if (requisicao.NovaSenha == requisicao.SenhaAtual)
                return Results.BadRequest();

            funcionario.HashSenha = hasher.HashPassword(funcionario, requisicao.NovaSenha);
            funcionario.PrimeiroAcesso = false;
            repositorio.UpdateAsync(funcionario);
            await repositorio.SaveChangesAsync();

            return Results.NoContent();
        })
            .WithSummary("Alterar senha do funcionário")
            .WithDescription(
                "Permite que um Funcionario autenticado troque a própria senha, informando a senha atual e a nova. " +
                "É o único endpoint acessível enquanto o primeiro acesso ainda não foi concluído.")
            .RequireAuthorization(policy => policy.RequireRole(Papeis.Funcionario));
    }
}
