using Refeicao.Core.Abstractions.Repositories;
using Refeicao.Core.Database.Entities;

namespace Refeicao.Tests.TestDoubles;

public class FakeAcompanhamentoRepository : IAcompanhamentoRepository
{
    private readonly Dictionary<int, Acompanhamento> _acompanhamentos = new();
    private int _proximoId = 1;

    public void Adicionar(Acompanhamento acompanhamento) => _acompanhamentos[acompanhamento.IdAcompanhamento] = acompanhamento;

    public Task<Acompanhamento?> GetByIdAsync(int id) =>
        Task.FromResult(_acompanhamentos.GetValueOrDefault(id));

    public Task<Acompanhamento?> GetByDescricaoAsync(string descricao) =>
        Task.FromResult(_acompanhamentos.Values.FirstOrDefault(a => a.DsAcompanhamento == descricao));

    public Task<IReadOnlyList<Acompanhamento>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Acompanhamento>>(_acompanhamentos.Values.ToList());

    public Task AddAsync(Acompanhamento acompanhamento)
    {
        acompanhamento.IdAcompanhamento = _proximoId++;
        _acompanhamentos[acompanhamento.IdAcompanhamento] = acompanhamento;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
