using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries
{
    public class GetAllEntitiesQuery : IRequest<IEnumerable<Entity>>
    {
    }

    public class GetAllEntitiesQueryHandler : IRequestHandler<GetAllEntitiesQuery, IEnumerable<Entity>>
    {
        private readonly IEntityRepository _repository;

        public GetAllEntitiesQueryHandler(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Entity>> Handle(GetAllEntitiesQuery request, CancellationToken cancellationToken)
        {
            // Retrieve all entities using the repository
            return await _repository.GetAllAsync();
        }
    }
}