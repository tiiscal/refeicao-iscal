using Microsoft.EntityFrameworkCore;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Context;
using RefeicaoEntity = Refeicao.Core.Database.Entities.Refeicao;

namespace Refeicao.Core.Database.Repositories;

public class RefeicaoRepository(RefeicaoContext context) : IRefeicaoRepository
{
    public Task<RefeicaoEntity?> GetByIdAsync(int id) =>
        context.Refeicoes.FirstOrDefaultAsync(r => r.IdRefeicao == id);

    public Task<IReadOnlyList<RefeicaoEntity>> GetAllWithAcompanhamentosAsync() =>
        context.Refeicoes
            .Include(r => r.AcompanhamentosRefeicao)
            .ThenInclude(ar => ar.Acompanhamento)
            .AsNoTracking()
            .ToListAsync()
            .ContinueWith(t => (IReadOnlyList<RefeicaoEntity>)t.Result);

    public async Task AddAsync(RefeicaoEntity refeicao) => await context.Refeicoes.AddAsync(refeicao);

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
}
