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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QualyticsDbContext _Con;

        public IUserRepository Users { get; }
        public IGenericRepository<RawData> RawDatas { get; }
        public IGenericRepository<DailyStat> DailyStats { get; }
        public UnitOfWork(QualyticsDbContext Con)
        {
            _Con = Con;
            Users = new UserRepository(_Con);
            RawDatas = new GenericRepository<RawData>(_Con);
            DailyStats = new GenericRepository<DailyStat>(_Con);
        }
        public async Task<int> CompleteAsync()
        {
            return await _Con.SaveChangesAsync();
        }
        public void Dispose()
        {
            _Con.Dispose();
        }
    }
}
