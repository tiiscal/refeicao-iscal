using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Refeicao.Api.Contracts;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;
using Refeicao.Core.Services;

namespace Refeicao.Api.Endpoints;

public static class UsuariosEndpoints
{
    public static void MapUsuariosEndpoints(this IEndpointRouteBuilder app)
    {
        var usuariosGroup = app.MapGroup("/api/usuarios").WithTags("Usuarios");

        usuariosGroup.MapGet("/", async (IUsuarioRepository repositorio) =>
        {
            var usuarios = await repositorio.GetAllAsync();
            var resposta = usuarios.Select(u => new UsuarioListResponse(u.IdUsuario, u.NmUsuario, u.DtCadastro)).ToList();
            return Results.Ok(resposta);
        })
            .AllowAnonymous()
            .WithSummary("Listar todos os usuários")
            .WithDescription("Retorna uma lista de todos os usuários cadastrados com IdUsuario, NmUsuario e DtCadastro.");

        usuariosGroup.MapPost("/", async (
            CadastroUsuarioRequest requisicao,
            IOptions<CadastroOptions> cadastroOptions,
            IUsuarioRepository repositorio,
            IPasswordHasher<Usuario> hasher) =>
        {
            if (!ChaveValida(requisicao.Chave, cadastroOptions.Value.Chave))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(requisicao.IdUsuario) ||
                string.IsNullOrWhiteSpace(requisicao.NmUsuario) ||
                string.IsNullOrWhiteSpace(requisicao.Senha))
                return Results.BadRequest();

            if (await repositorio.GetByIdAsync(requisicao.IdUsuario) is not null)
                return Results.Conflict();

            var usuario = new Usuario
            {
                IdUsuario = requisicao.IdUsuario,
                NmUsuario = requisicao.NmUsuario,
                DtCadastro = DateTime.Now
            };
            usuario.HashSenha = hasher.HashPassword(usuario, requisicao.Senha);

            await repositorio.AddAsync(usuario);
            await repositorio.SaveChangesAsync();

            return Results.Created(
                "/api/auth/usuario/login", new CadastroUsuarioResponse(usuario.IdUsuario, usuario.NmUsuario));
        })
            .AllowAnonymous()
            .WithSummary("Cadastrar usuário")
            .WithDescription(
                "Cria uma nova conta de Usuario. Exige a chave ADMIN_KEY válida no corpo da requisição. " +
                "Retorna 409 se o id já estiver em uso. Não faz login automático — use /api/auth/usuario/login depois.");

        usuariosGroup.MapDelete("/{idUsuario}", async (
            string idUsuario,
            [FromQuery] string? chave,
            IOptions<CadastroOptions> cadastroOptions,
            IUsuarioRepository repositorio) =>
        {
            if (!ChaveValida(chave, cadastroOptions.Value.Chave))
                return Results.Unauthorized();

            var usuario = await repositorio.GetByIdAsync(idUsuario);
            if (usuario is null)
                return Results.NotFound(new { message = "Usuário não encontrado" });

            if (usuario.DtInativacao is not null)
                return Results.BadRequest(new { message = "Usuário já desativado" });

            usuario.DtInativacao = DateTime.Now;
            repositorio.UpdateAsync(usuario);
            await repositorio.SaveChangesAsync();

            return Results.NoContent();
        })
            .AllowAnonymous()
            .WithSummary("Desativar usuário")
            .WithDescription(
                "Desativa uma conta de Usuario marcando a data de inativação. Exige a chave ADMIN_KEY " +
                "como query string (?chave=...). Retorna 404 se o usuário não existir, " +
                "400 se já estiver desativado.");
    }

    private static bool ChaveValida(string? chaveRecebida, string chaveConfigurada)
    {
        var recebida = Encoding.UTF8.GetBytes(chaveRecebida ?? string.Empty);
        var configurada = Encoding.UTF8.GetBytes(chaveConfigurada);

        return recebida.Length == configurada.Length && CryptographicOperations.FixedTimeEquals(recebida, configurada);
    }
}
