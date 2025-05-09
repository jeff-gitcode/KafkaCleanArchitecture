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
        public Task<Entity> Handle(GetEntityQuery request, CancellationToken cancellationToken)
        {
            // Replace with your actual logic to retrieve the entity
            var entity = new Entity
            {
                Id = request.Id,
                Name = "Sample Entity"
            };

            return Task.FromResult(entity);
        }
    }
}