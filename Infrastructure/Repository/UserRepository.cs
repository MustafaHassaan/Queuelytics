using Application.Interfaces;
using Domain.Models;
using Infrastructure.Dataaccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly QualyticsDbContext _Con;
        private readonly DbSet<User> _Dbs;
        public UserRepository(QualyticsDbContext Con) : base(Con)
        {
            _Con = Con;
            _Dbs = _Con.Set<User>();
        }
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _Dbs.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
