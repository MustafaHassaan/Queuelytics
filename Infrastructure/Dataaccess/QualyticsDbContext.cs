using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Dataaccess
{
    public class QualyticsDbContext : DbContext
    {
        public QualyticsDbContext(DbContextOptions<QualyticsDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RawData> RawDatas { get; set; }
        public DbSet<DailyStat> DailyStats { get; set; }
    }
}
