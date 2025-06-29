using BankingProject.Domain.Abstractions;

namespace BankingProject.Domain.Context.CustomerAggregate.Repositories;

public interface IGenericRepository<TEntity> where TEntity : IAggregateRoot
{
    public Task SaveAsync(TEntity entity);
}
