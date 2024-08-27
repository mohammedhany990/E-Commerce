using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo.BBL.Interfaces;
using Demo.DAL.Data.Contexts;
using Demo.DAL.Models;

namespace Demo.BBL.Repositories
{
    public class AppUserRepository : GenericRepository<AppUser>, IAppUserRepository
    {
        public AppUserRepository(ECommerceDbContext dbContext) : base(dbContext)
        {

        }
    }
}
