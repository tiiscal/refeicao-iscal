using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Abstractions.Repositories;

public interface IAcompanhamentoRepository
{
    Task<Acompanhamento?> GetByIdAsync(int id);
    Task<Acompanhamento?> GetByDescricaoAsync(string descricao);
    Task<IReadOnlyList<Acompanhamento>> GetAllAsync();
    Task AddAsync(Acompanhamento acompanhamento);
    Task<int> SaveChangesAsync();
}
