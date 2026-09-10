using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.TestDoubles;

public class FakeAcompanhamentoRefeicaoRepository : IAcompanhamentoRefeicaoRepository
{
    private readonly Dictionary<(int, int), AcompanhamentoRefeicao> _acompanhamentosRefeicao = new();

    public Task<AcompanhamentoRefeicao?> GetByIdRefeicaoAsync(int idRefeicao) =>
        Task.FromResult(_acompanhamentosRefeicao.Values.FirstOrDefault(x => x.IdRefeicao == idRefeicao));

    public Task<AcompanhamentoRefeicao?> GetByIdAsync((int idAcompanhamento, int idRefeicao) id) =>
        Task.FromResult(_acompanhamentosRefeicao.GetValueOrDefault(id));

    public Task AddAsync(AcompanhamentoRefeicao acompanhamentoRefeicao)
    {
        _acompanhamentosRefeicao[(acompanhamentoRefeicao.IdAcompanhamento, acompanhamentoRefeicao.IdRefeicao)] = acompanhamentoRefeicao;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
