using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Refeicao.Api.Contracts;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;
using Refeicao.Core.Services;
using System.Security.Cryptography;
using System.Text;

namespace Refeicao.Api.Endpoints;

public static class FuncionariosEndpoints
{
    public static void MapFuncionariosEndpoints(this IEndpointRouteBuilder app)
    {
        var funcionariosGroup = app.MapGroup("/api/funcionarios").WithTags("Funcionarios");

        funcionariosGroup.MapGet("/", async (IFuncionarioRepository repositorio) =>
        {
            var funcionarios = await repositorio.GetAllAsync();
            var resposta = funcionarios.Select(f => new FuncionarioListResponse(f.IdFuncionario, f.NmUsuario, f.DtCadastro)).ToList();
            return Results.Ok(resposta);
        })
            .AllowAnonymous()
            .WithSummary("Listar todos os funcionários")
            .WithDescription("Retorna uma lista de todos os funcionários cadastrados com IdFuncionario, NmUsuario e DtCadastro.");

        funcionariosGroup.MapPost("/", async (
            CadastroFuncionarioRequest requisicao,
            IOptions<CadastroOptions> cadastroOptions,
            IFuncionarioRepository repositorio,
            IPasswordHasher<Funcionario> hasher) =>
        {
            if (!ChaveValida(requisicao.Chave, cadastroOptions.Value.Chave))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(requisicao.NmUsuario) || string.IsNullOrWhiteSpace(requisicao.Senha))
                return Results.BadRequest();

            if (await repositorio.GetByUsernameAsync(requisicao.NmUsuario) is not null)
                return Results.Conflict();

            var funcionario = new Funcionario
            {
                NmUsuario = requisicao.NmUsuario,
                DtCadastro = DateTime.Now,
            };
            funcionario.HashSenha = hasher.HashPassword(funcionario, requisicao.Senha);

            await repositorio.AddAsync(funcionario);
            await repositorio.SaveChangesAsync();

            return Results.Created(
                "/api/auth/funcionario/login",
                new CadastroFuncionarioResponse(funcionario.IdFuncionario, funcionario.NmUsuario));
        })
            .AllowAnonymous()
            .WithSummary("Cadastrar funcionário")
            .WithDescription(
                "Cria uma nova conta de Funcionario. Exige a chave ADMIN_KEY válida no corpo da requisição. " +
                "Retorna 409 se o nome de usuário já estiver em uso. Não faz login automático — " +
                "use /api/auth/funcionario/login depois.");

        funcionariosGroup.MapDelete("/{idFuncionario:int}", async (
            int idFuncionario,
            [FromQuery] string? chave,
            IOptions<CadastroOptions> cadastroOptions,
            IFuncionarioRepository repositorio) =>
                {
                    if (!ChaveValida(chave, cadastroOptions.Value.Chave))
                        return Results.Unauthorized();

                    var funcionario = await repositorio.GetByIdAsync(idFuncionario);
                    if (funcionario is null)
                        return Results.NotFound(new { message = "Funcionário não encontrado" });

                    if (funcionario.DtInativacao is not null)
                        return Results.BadRequest(new { message = "Funcionário já desativado" });

                    funcionario.DtInativacao = DateTime.Now;
                    repositorio.UpdateAsync(funcionario);
                    await repositorio.SaveChangesAsync();

                    return Results.NoContent();
                })
            .AllowAnonymous()
            .WithSummary("Desativar funcionário")
            .WithDescription(
                "Desativa uma conta de Funcionario marcando a data de inativação. Exige a chave de administrador " +
                "como query string (?chave=...). Retorna 404 se o funcionário não existir, " +
                "400 se já estiver desativado.");
            }

    private static bool ChaveValida(string? chaveRecebida, string chaveConfigurada)
    {
        var recebida = Encoding.UTF8.GetBytes(chaveRecebida ?? string.Empty);
        var configurada = Encoding.UTF8.GetBytes(chaveConfigurada);

        return recebida.Length == configurada.Length && CryptographicOperations.FixedTimeEquals(recebida, configurada);
    }
}
