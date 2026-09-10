using Microsoft.EntityFrameworkCore;
using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Context;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Core.Database.Repositories;

public class UsuarioRepository(RefeicaoContext context) : IUsuarioRepository
{
    public Task<Usuario?> GetByIdAsync(string id) =>
        context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);

    public async Task<IReadOnlyList<Usuario>> GetAllAsync() =>
        await context.Usuarios.AsNoTracking().ToListAsync();

    public async Task AddAsync(Usuario usuario) => await context.Usuarios.AddAsync(usuario);

    public void UpdateAsync(Usuario usuario) => context.Usuarios.Update(usuario);

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
}
