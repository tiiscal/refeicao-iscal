using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Abstractions.Repositories;

public interface IFuncionarioRepository
{
    Task<Funcionario?> GetByIdAsync(int id);
    Task<Funcionario?> GetByUsernameAsync(string nmUsuario);
    Task<IReadOnlyList<Funcionario>> GetAllAsync();
    Task AddAsync(Funcionario funcionario);
    void UpdateAsync(Funcionario funcionario);
    Task<int> SaveChangesAsync();
}
