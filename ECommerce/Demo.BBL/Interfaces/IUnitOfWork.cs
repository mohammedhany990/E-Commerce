using Demo.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.BBL.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<T> Repository<T>() where T : BaseEntity;
        Task<int> CompleteAsync();
        public void DetachEntity<T>(T entity) where T : BaseEntity;
        public IOrderHeaderRepository OrderHeaderRepository { get; set; }
        IAppUserRepository AppUserRepository { get; set; }

    }
}
