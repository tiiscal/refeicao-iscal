using Microsoft.EntityFrameworkCore;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Context;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Database.Repositories;

public class AcompanhamentoRepository(RefeicaoContext context) : IAcompanhamentoRepository
{
    public Task<Acompanhamento?> GetByIdAsync(int id) =>
        context.Acompanhamentos.FirstOrDefaultAsync(a => a.IdAcompanhamento == id);

    public Task<Acompanhamento?> GetByDescricaoAsync(string descricao) =>
        context.Acompanhamentos.FirstOrDefaultAsync(a => a.DsAcompanhamento == descricao);

    public Task<IReadOnlyList<Acompanhamento>> GetAllAsync() =>
        context.Acompanhamentos
            .AsNoTracking()
            .ToListAsync()
            .ContinueWith(t => (IReadOnlyList<Acompanhamento>)t.Result);

    public async Task AddAsync(Acompanhamento acompanhamento) =>
        await context.Acompanhamentos.AddAsync(acompanhamento);

    public Task<int> SaveChangesAsync() =>
        context.SaveChangesAsync();
}
