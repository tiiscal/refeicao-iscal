using Microsoft.EntityFrameworkCore;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Context;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Database.Repositories;

public class AcompanhamentoRefeicaoRepository(RefeicaoContext context) : IAcompanhamentoRefeicaoRepository
{
    public Task<AcompanhamentoRefeicao?> GetByIdRefeicaoAsync(int idRefeicao) =>
        context.AcompanhamentosRefeicao.FirstOrDefaultAsync(x => x.IdRefeicao == idRefeicao);

    public Task<AcompanhamentoRefeicao?> GetByIdAsync((int idAcompanhamento, int idRefeicao) id) =>
        context.AcompanhamentosRefeicao.FirstOrDefaultAsync(x =>
            x.IdAcompanhamento == id.idAcompanhamento && x.IdRefeicao == id.idRefeicao);

    public async Task AddAsync(AcompanhamentoRefeicao acompanhamentoRefeicao) =>
        await context.AcompanhamentosRefeicao.AddAsync(acompanhamentoRefeicao);

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
}
