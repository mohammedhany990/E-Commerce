using Demo.BBL.Interfaces;
using Demo.DAL.Data.Contexts;
using Demo.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.BBL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ECommerceDbContext _dbContext;
        private Hashtable Repositories;
        public IOrderHeaderRepository OrderHeaderRepository { get; set; }
        public IAppUserRepository AppUserRepository { get; set; }

        public UnitOfWork(ECommerceDbContext dbContext)
        {
            _dbContext = dbContext;
            Repositories = new Hashtable();
            OrderHeaderRepository = new OrderHeaderRepository(dbContext);
            AppUserRepository = new AppUserRepository(dbContext);

        }

       
        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            var key = typeof(T).Name;
            if (!Repositories.ContainsKey(key))
            {
                var repo = new GenericRepository<T>(_dbContext);
                Repositories.Add(key, repo);
            }
            return (IGenericRepository<T>)Repositories[key];
        }

        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _dbContext.DisposeAsync();
        }

        public void DetachEntity<T>(T entity) where T : BaseEntity
        {
            _dbContext.Entry(entity).State = EntityState.Detached;
        }
        public ECommerceDbContext Context => _dbContext;

    }
}
