using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly DbSet<TEntity> _dBSet;
        public Repository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            _dBSet = _applicationDbContext.Set<TEntity>();
        }

        public async Task AddEntity(TEntity entity)
        {
            await _dBSet.AddAsync(entity);
        }

        public void DeleteEntityPermanently(TEntity entity)
        {
            _dBSet.Remove(entity);
        }

        public void DeleteEntitySoft(TEntity entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            this.UpdateEntity(entity);
        }

        public async ValueTask DisposeAsync()
        {
            if (_applicationDbContext != null)
            {
                await _applicationDbContext.DisposeAsync();
            }
        }

        public async Task<TEntity?> GetEntityById(long entityId)
        {
            return await _dBSet.Where(e => e.Id == entityId).SingleOrDefaultAsync();
        }

        public IQueryable<TEntity> GetQuery()
        {
            return _dBSet.AsQueryable();
        }

        public async Task SaveChanges()
        {
            await _applicationDbContext.SaveChangesAsync();
        }

        public void UpdateEntity(TEntity entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dBSet.Update(entity);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _applicationDbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _applicationDbContext.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _applicationDbContext.Database.RollbackTransactionAsync();
        }
    }
}