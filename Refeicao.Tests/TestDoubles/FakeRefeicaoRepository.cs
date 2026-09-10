using Refeicao.Core.Abstractions.Repositories;
using RefeicaoEntity = Refeicao.Core.Database.Entities.Refeicao;

namespace Refeicao.Tests.TestDoubles;

public class FakeRefeicaoRepository : IRefeicaoRepository
{
    private readonly Dictionary<int, RefeicaoEntity> _refeicoes = new();
    private int _proximoId = 1;

    public void Adicionar(RefeicaoEntity refeicao) => _refeicoes[refeicao.IdRefeicao] = refeicao;

    public Task<RefeicaoEntity?> GetByIdAsync(int id) =>
        Task.FromResult(_refeicoes.GetValueOrDefault(id));

    public Task<IReadOnlyList<RefeicaoEntity>> GetAllWithAcompanhamentosAsync() =>
        Task.FromResult<IReadOnlyList<RefeicaoEntity>>(_refeicoes.Values.ToList());

    public Task AddAsync(RefeicaoEntity refeicao)
    {
        refeicao.IdRefeicao = _proximoId++;
        _refeicoes[refeicao.IdRefeicao] = refeicao;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
