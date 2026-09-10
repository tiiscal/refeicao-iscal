using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Abstractions.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(string id);
    Task<IReadOnlyList<Usuario>> GetAllAsync();
    Task AddAsync(Usuario usuario);
    void UpdateAsync(Usuario usuario);
    Task<int> SaveChangesAsync();
}
