using Microsoft.EntityFrameworkCore;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Context;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Database.Repositories;

public class CardapioRepository(RefeicaoContext context) : ICardapioRepository
{
    public async Task<IReadOnlyList<Cardapio>> GetByPeriodoAsync(DateOnly inicio, DateOnly fim) =>
        await context.Cardapios.AsNoTracking()
            .Where(c => c.DtCardapio >= inicio && c.DtCardapio <= fim)
            .Include(c => c.Refeicao!)
                .ThenInclude(r => r.AcompanhamentosRefeicao)
                .ThenInclude(ar => ar.Acompanhamento)
            .OrderBy(c => c.DtCardapio)
            .ThenBy(c => c.TpCardapio)
            .ToListAsync();

    public Task<bool> ExistePorDataETipoAsync(DateOnly dtCardapio, TipoCardapio tpCardapio) =>
        context.Cardapios.AnyAsync(c => c.DtCardapio == dtCardapio && c.TpCardapio == tpCardapio);

    public async Task AddAsync(Cardapio cardapio) => await context.Cardapios.AddAsync(cardapio);

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
}
