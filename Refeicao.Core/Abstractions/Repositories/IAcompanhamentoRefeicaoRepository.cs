using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Abstractions.Repositories;

public interface IAcompanhamentoRefeicaoRepository
{
    Task<AcompanhamentoRefeicao?> GetByIdRefeicaoAsync(int idRefeicao);
    Task<AcompanhamentoRefeicao?> GetByIdAsync((int idAcompanhamento, int idRefeicao) id);
    Task AddAsync(AcompanhamentoRefeicao acompanhamentoRefeicao);
    Task<int> SaveChangesAsync();
}
