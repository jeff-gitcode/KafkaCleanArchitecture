using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEntityRepository
    {
        Task<Entity> GetByIdAsync(Guid id);
        Task<IEnumerable<Entity>> GetAllAsync();
        Task AddAsync(Entity entity);
        Task UpdateAsync(Entity entity);
        Task DeleteAsync(Guid id);
    }
}