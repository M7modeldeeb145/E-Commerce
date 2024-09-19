using DeeboStore.DataAccess.Data;
using DeeboStore.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DeeboStore.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext context;
        internal DbSet<T> dbset;
        public Repository(ApplicationDbContext context)
        {
            this.context = context;
            this.dbset = context.Set<T>();
            context.Products.Include(e => e.Category);
        }
        public void Create(T entity)
        {
            dbset.Add(entity);
        }
        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> values;
            if (tracked = true)
            {
                values = dbset;
            }
            else
            {
               values = dbset.AsNoTracking();
            }
            values = values.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    values = values.Include(property);
                }
            }
            return values.FirstOrDefault();
        }
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> values = dbset;
            if(filter != null)
            {
                values = values.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach(var property in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    values = values.Include(property);
                }
            }
            return values.ToList();
        }
        public void Remove(T entity)
        {
            dbset.Remove(entity);
        }
        public void RemoveRange(IEnumerable<T> entities)
        {
            dbset.RemoveRange(entities);
        }
    }
}