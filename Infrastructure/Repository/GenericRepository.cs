using Application.Interfaces;
using Infrastructure.Dataaccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly QualyticsDbContext _Con;
        private readonly DbSet<T> _Dbs;
        public GenericRepository(QualyticsDbContext Con)
        {
            _Con = Con;
            _Dbs = _Con.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _Dbs.ToListAsync();
        }
        public async Task<T?> GetByDateAsync(DateOnly date)
        {
            return await _Dbs.FirstOrDefaultAsync(x => EF.Property<DateOnly>(x, "Date") == date);
        }
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _Dbs.FindAsync(id);
        }
        public async Task AddAsync(T entity)
        {
            await _Dbs.AddAsync(entity);
        }
        public void Update(T entity)
        {
            _Dbs.Update(entity);
        }
        public void Remove(T entity)
        {
            _Dbs.Remove(entity);
        }
    }
}
