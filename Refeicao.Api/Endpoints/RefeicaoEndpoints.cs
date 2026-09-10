using System.Security.Claims;
using Refeicao.Api.Authentication;
using Refeicao.Api.Contracts;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;
using RefeicaoEntity = Refeicao.Core.Database.Entities.Refeicao;

namespace Refeicao.Api.Endpoints;

public static class RefeicaoEndpoints
{
    public static void MapRefeicaoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/refeicao")
            .WithTags("Refeição")
            .RequireAuthorization(policy => policy
                .RequireRole(Papeis.Usuario)
                .AddRequirements(new SenhaAlteradaRequirement()));

        grupo.MapGet("/", async (IRefeicaoRepository refeicaoRepositorio) =>
        {
            var refeicoes = await refeicaoRepositorio.GetAllWithAcompanhamentosAsync();
            var response = refeicoes.Select(r => new RefeicaoComAcompanhamentosResponse(
                r.IdRefeicao,
                r.DsRefeicao,
                r.AcompanhamentosRefeicao
                    .Select(ar => new AcompanhamentoItem(ar.Acompanhamento!.IdAcompanhamento, ar.Acompanhamento.DsAcompanhamento))
                    .ToList()
            )).ToList();

            return Results.Ok(response);
        })
            .WithSummary("Listar refeições")
            .WithDescription(
                "Retorna a lista de todas as refeições cadastradas com seus respectivos complementos (acompanhamentos).");

        grupo.MapPost("/{idRefeicao:int}/acompanhamentos", async (
            int idRefeicao,
            CadastrarAcompanhamentoRefeicaoRequest requisicao,
            IRefeicaoRepository refeicaoRepositorio,
            IAcompanhamentoRefeicaoRepository acompRefeicaoRepositorio) =>
        {
            var refeicao = await refeicaoRepositorio.GetByIdAsync(idRefeicao);
            if (refeicao is null)
                return Results.NotFound("Refeição não encontrada");

            var acompanhamentoExistente = await acompRefeicaoRepositorio.GetByIdAsync((requisicao.IdAcompanhamento, idRefeicao));
            if (acompanhamentoExistente is not null)
                return Results.Conflict("Acompanhamento já cadastrado para esta refeição");

            var acompanhamentoRefeicao = new AcompanhamentoRefeicao
            {
                IdAcompanhamento = requisicao.IdAcompanhamento,
                IdRefeicao = idRefeicao,
            };

            await acompRefeicaoRepositorio.AddAsync(acompanhamentoRefeicao);
            await acompRefeicaoRepositorio.SaveChangesAsync();

            return Results.Created(
                $"/api/refeicao/{idRefeicao}/acompanhamentos/{requisicao.IdAcompanhamento}",
                new { IdAcompanhamento = requisicao.IdAcompanhamento });
        })
            .WithSummary("Cadastrar acompanhamento a uma refeição")
            .WithDescription(
                "Adiciona um complemento (acompanhamento) a uma refeição existente. " +
                "Retorna 404 se a refeição não existir, 409 se o acompanhamento já estiver associado.");

        grupo.MapPost("/", async (
            CadastrarRefeicaoRequest requisicao,
            ClaimsPrincipal usuarioLogado,
            IUsuarioRepository usuarioRepositorio,
            IRefeicaoRepository refeicaoRepositorio) =>
        {
            if (string.IsNullOrWhiteSpace(requisicao.DsRefeicao))
                return Results.BadRequest();

            var idUsuario = usuarioLogado.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var usuario = await usuarioRepositorio.GetByIdAsync(idUsuario);
            if (usuario is null || usuario.DtInativacao is not null)
                return Results.Unauthorized();

            var refeicao = new RefeicaoEntity
            {
                DsRefeicao = requisicao.DsRefeicao,
                IdUsuario = idUsuario,
            };

            await refeicaoRepositorio.AddAsync(refeicao);
            await refeicaoRepositorio.SaveChangesAsync();

            return Results.Created(
                $"/api/refeicao/{refeicao.IdRefeicao}", new RefeicaoResponse(refeicao.IdRefeicao, refeicao.DsRefeicao));
        })
            .WithSummary("Cadastrar refeição")
            .WithDescription(
                "Cria uma refeição (ex.: a descrição de um prato) que poderá ser usada posteriormente em um " +
                "cardápio. Restrito a contas Usuario com a senha já alterada.");
    }
}
