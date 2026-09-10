using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.TestDoubles;

public class FakeFuncionarioRepository : IFuncionarioRepository
{
    private readonly Dictionary<int, Funcionario> _funcionarios = new();

    public Dictionary<int, Funcionario> Funcionarios => _funcionarios;

    public void Adicionar(Funcionario funcionario) => _funcionarios[funcionario.IdFuncionario] = funcionario;

    public Task<Funcionario?> GetByIdAsync(int id) =>
        Task.FromResult(_funcionarios.GetValueOrDefault(id));

    public Task<Funcionario?> GetByUsernameAsync(string nmUsuario) =>
        Task.FromResult(_funcionarios.Values.FirstOrDefault(f => f.NmUsuario == nmUsuario));

    public Task<IReadOnlyList<Funcionario>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Funcionario>>(_funcionarios.Values.ToList());

    public Task AddAsync(Funcionario funcionario)
    {
        _funcionarios[funcionario.IdFuncionario] = funcionario;
        return Task.CompletedTask;
    }

    public void UpdateAsync(Funcionario funcionario) => _funcionarios[funcionario.IdFuncionario] = funcionario;

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
