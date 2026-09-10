using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.TestDoubles;

public class ThrowingUsuarioRepository : IUsuarioRepository
{
    public const string MensagemErro = "falha simulada de banco de dados";

    public Task<Usuario?> GetByIdAsync(string id) => throw new InvalidOperationException(MensagemErro);

    public Task<IReadOnlyList<Usuario>> GetAllAsync() => throw new InvalidOperationException(MensagemErro);

    public Task AddAsync(Usuario usuario) => throw new InvalidOperationException(MensagemErro);

    public void UpdateAsync(Usuario usuario) => throw new InvalidOperationException(MensagemErro);

    public Task<int> SaveChangesAsync() => throw new InvalidOperationException(MensagemErro);
}
