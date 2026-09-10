using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Refeicao.Api.Authentication;
using Refeicao.Api.Contracts;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Api.Endpoints;

public static class AcompanhamentoEndpoints
{
    public static void MapAcompanhamentoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/acompanhamentos")
            .WithTags("Acompanhamentos")
            .RequireAuthorization(policy => policy.RequireRole(Papeis.Usuario));

        grupo.MapGet("/", async (IAcompanhamentoRepository repositorio) =>
        {
            var acompanhamentos = await repositorio.GetAllAsync();
            var response = acompanhamentos.Select(a =>
                new CadastroAcompanhamentoResponse(a.IdAcompanhamento, a.DsAcompanhamento)).ToList();

            return Results.Ok(response);
        })
            .WithName("ListarAcompanhamentos")
            .WithSummary("Listar acompanhamentos")
            .WithDescription("Retorna a lista de todos os acompanhamentos (complementos) cadastrados.")
            .Produces(200)
            .Produces(401);

        grupo.MapPost("/", async (
            CadastroAcompanhamentoRequest requisicao,
            ClaimsPrincipal usuarioLogado,
            IAcompanhamentoRepository repositorio) =>
        {
            if (string.IsNullOrWhiteSpace(requisicao.DsAcompanhamento))
                return Results.BadRequest();

            if (await repositorio.GetByDescricaoAsync(requisicao.DsAcompanhamento) is not null)
                return Results.Conflict();

            var idUsuario = usuarioLogado.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var acompanhamento = new Acompanhamento
            {
                DsAcompanhamento = requisicao.DsAcompanhamento,
                IdUsuario = idUsuario,
                SnDeletado = false
            };

            await repositorio.AddAsync(acompanhamento);
            await repositorio.SaveChangesAsync();

            return Results.Created(
                $"/api/acompanhamentos/{acompanhamento.IdAcompanhamento}",
                new CadastroAcompanhamentoResponse(acompanhamento.IdAcompanhamento, acompanhamento.DsAcompanhamento));
        })
            .WithName("CriarAcompanhamento")
            .WithSummary("Criar acompanhamento")
            .WithDescription(
                "Cria um novo acompanhamento (complemento). Requer autenticação como usuário. " +
                "Retorna 409 Conflict se a descrição já estiver em uso.")
            .Produces(201)
            .Produces(400)
            .Produces(401)
            .Produces(409);

        grupo.MapGet("/{id}", async (
            int id,
            IAcompanhamentoRepository repositorio) =>
        {
            var acompanhamento = await repositorio.GetByIdAsync(id);

            if (acompanhamento is null)
                return Results.NotFound();

            return Results.Ok(
                new CadastroAcompanhamentoResponse(acompanhamento.IdAcompanhamento, acompanhamento.DsAcompanhamento));
        })
            .WithName("ObterAcompanhamento")
            .WithSummary("Obter acompanhamento por ID")
            .WithDescription("Retorna um acompanhamento específico pelo seu ID. Requer autenticação como usuário.")
            .Produces(200)
            .Produces(401)
            .Produces(404);
    }
}
