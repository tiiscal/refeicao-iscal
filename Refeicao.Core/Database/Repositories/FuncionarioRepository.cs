using Microsoft.EntityFrameworkCore;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Context;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Database.Repositories;

public class FuncionarioRepository(RefeicaoContext context) : IFuncionarioRepository
{
    public Task<Funcionario?> GetByIdAsync(int id) =>
        context.Funcionarios.FirstOrDefaultAsync(f => f.IdFuncionario == id);

    public Task<Funcionario?> GetByUsernameAsync(string nmUsuario) =>
        context.Funcionarios.FirstOrDefaultAsync(f => f.NmUsuario == nmUsuario);

    public async Task<IReadOnlyList<Funcionario>> GetAllAsync() =>
        await context.Funcionarios.AsNoTracking().ToListAsync();

    public async Task AddAsync(Funcionario funcionario) => await context.Funcionarios.AddAsync(funcionario);

    public void UpdateAsync(Funcionario funcionario) => context.Funcionarios.Update(funcionario);

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
}
