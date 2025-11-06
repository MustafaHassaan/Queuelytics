using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IGenericRepository<RawData> RawDatas { get; }
        IGenericRepository<DailyStat> DailyStats { get; }
        Task<int> CompleteAsync();
    }
}
