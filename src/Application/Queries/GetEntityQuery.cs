using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Queries
{
    public class GetEntityQuery : IRequest<Entity>
    {
        public Guid Id { get; set; }
    }

    public class GetEntityQueryHandler : IRequestHandler<GetEntityQuery, Entity>
    {
        private readonly IEntityRepository _repository;

        public GetEntityQueryHandler(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<Entity> Handle(GetEntityQuery request, CancellationToken cancellationToken)
        {
            // Retrieve the entity using the repository
            return await _repository.GetByIdAsync(request.Id);
        }
    }
}