using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class EntityRepository : IEntityRepository
    {
        private readonly AppDbContext _dbContext;

        public EntityRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Entity> GetByIdAsync(Guid id)
        {
            return await _dbContext.Entities.FindAsync(id);
        }

        public async Task<IEnumerable<Entity>> GetAllAsync()
        {
            return await _dbContext.Entities.ToListAsync();
        }

        public async Task AddAsync(Entity entity)
        {
            await _dbContext.Entities.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Entity entity)
        {
            _dbContext.Entities.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbContext.Entities.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
