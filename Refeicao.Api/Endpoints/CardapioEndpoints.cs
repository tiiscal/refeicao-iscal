using System.Security.Claims;
using Refeicao.Api.Authentication;
using Refeicao.Api.Contracts;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Api.Endpoints;

public static class CardapioEndpoints
{
    public static void MapCardapioEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cardapio/semana-atual", async (ICardapioRepository repositorio) =>
                Results.Ok(await ObterCardapioSemanaAtualAsync(repositorio)))
            .WithTags("Cardápio")
            .WithSummary("Consultar cardápio da semana atual")
            .WithDescription(
                "Endpoint público (sem autenticação) que lista os cardápios cadastrados para a semana atual " +
                "(segunda a domingo), com a refeição e os acompanhamentos de cada dia.")
            .AllowAnonymous();

        var grupo = app.MapGroup("/api/cardapio")
            .WithTags("Cardápio")
            .RequireAuthorization(policy => policy
                .RequireRole(Papeis.Usuario)
                .AddRequirements(new SenhaAlteradaRequirement()));

        grupo.MapPost("/", async (
            CadastrarCardapioRequest requisicao,
            ClaimsPrincipal usuarioLogado,
            IUsuarioRepository usuarioRepositorio,
            IRefeicaoRepository refeicaoRepositorio,
            ICardapioRepository cardapioRepositorio) =>
        {
            if (!Enum.TryParse<TipoCardapio>(requisicao.TpCardapio, out var tipoCardapio))
                return Results.BadRequest();

            var idUsuario = usuarioLogado.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var usuario = await usuarioRepositorio.GetByIdAsync(idUsuario);
            if (usuario is null || usuario.DtInativacao is not null)
                return Results.Unauthorized();

            if (await refeicaoRepositorio.GetByIdAsync(requisicao.IdRefeicao) is null)
                return Results.BadRequest();

            if (await cardapioRepositorio.ExistePorDataETipoAsync(requisicao.DtCardapio, tipoCardapio))
                return Results.Conflict();

            var cardapio = new Cardapio
            {
                TpCardapio = tipoCardapio,
                DtCardapio = requisicao.DtCardapio,
                IdUsuario = idUsuario,
                IdRefeicao = requisicao.IdRefeicao,
            };

            await cardapioRepositorio.AddAsync(cardapio);
            await cardapioRepositorio.SaveChangesAsync();

            return Results.Created(
                $"/api/cardapio/{cardapio.IdCardapio}",
                new CardapioResponse(
                    cardapio.IdCardapio,
                    cardapio.DtCardapio,
                    cardapio.TpCardapio.ToString(),
                    cardapio.IdRefeicao,
                    cardapio.SnFechado));
        })
            .WithSummary("Cadastrar cardápio")
            .WithDescription(
                "Cria um cardápio para uma data e tipo (\"J\" Janta / \"A\" Almoço), vinculado a uma refeição já " +
                "cadastrada. Restrito a contas Usuario com a senha já alterada. Retorna 409 se já existir um " +
                "cardápio para essa data e tipo, e 400 se a refeição informada não existir.");
    }

    private static async Task<IReadOnlyList<CardapioSemanaResponse>> ObterCardapioSemanaAtualAsync(
        ICardapioRepository repositorio)
    {
        var (inicio, fim) = ObterSemanaAtual(DateOnly.FromDateTime(DateTime.Now));
        var cardapios = await repositorio.GetByPeriodoAsync(inicio, fim);

        return cardapios.Select(c => new CardapioSemanaResponse(
            c.IdCardapio,
            c.DtCardapio,
            c.TpCardapio.ToString(),
            c.Refeicao?.DsRefeicao ?? string.Empty,
            c.Refeicao?.AcompanhamentosRefeicao
                .Select(ar => ar.Acompanhamento?.DsAcompanhamento ?? string.Empty)
                .ToList() ?? [],
            c.SnFechado)).ToList();
    }

    private static (DateOnly Inicio, DateOnly Fim) ObterSemanaAtual(DateOnly hoje)
    {
        var diaSemana = (int)hoje.DayOfWeek;
        var diasDesdeSegunda = diaSemana == 0 ? 6 : diaSemana - 1;
        var inicio = hoje.AddDays(-diasDesdeSegunda);
        return (inicio, inicio.AddDays(6));
    }
}
