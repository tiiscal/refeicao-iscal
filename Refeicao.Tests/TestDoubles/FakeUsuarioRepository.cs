using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.TestDoubles;

public class FakeUsuarioRepository : IUsuarioRepository
{
    private readonly Dictionary<string, Usuario> _usuarios = new();

    public void Adicionar(Usuario usuario) => _usuarios[usuario.IdUsuario] = usuario;

    public Task<Usuario?> GetByIdAsync(string id) =>
        Task.FromResult(_usuarios.GetValueOrDefault(id));

    public Task<IReadOnlyList<Usuario>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Usuario>>(_usuarios.Values.ToList());

    public Task AddAsync(Usuario usuario)
    {
        _usuarios[usuario.IdUsuario] = usuario;
        return Task.CompletedTask;
    }

    public void UpdateAsync(Usuario usuario) => _usuarios[usuario.IdUsuario] = usuario;

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
