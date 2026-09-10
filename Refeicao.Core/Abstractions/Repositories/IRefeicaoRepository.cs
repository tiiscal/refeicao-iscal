using RefeicaoEntity = Refeicao.Core.Database.Entities.Refeicao;

namespace Refeicao.Core.Abstractions.Repositories;

public interface IRefeicaoRepository
{
    Task<RefeicaoEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<RefeicaoEntity>> GetAllWithAcompanhamentosAsync();
    Task AddAsync(RefeicaoEntity refeicao);
    Task<int> SaveChangesAsync();
}
