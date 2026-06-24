using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Repository;

public interface IRepository<TEntity> : IAsyncDisposable
{
    // just CRUD.. if we want to do more operations -> get DbSet from it's repository.GetQuery() & do whatever you want
    public Task<TEntity?> GetEntityById(int id);
    public Task AddEntity(TEntity entity);

    public void DeleteEntitySoft(TEntity entity);
    public void DeleteEntityPermanently(TEntity entity);
    public void UpdateEntity(TEntity entity);

    public IQueryable<TEntity> GetQuery();

    public Task SaveChanges();
    
}