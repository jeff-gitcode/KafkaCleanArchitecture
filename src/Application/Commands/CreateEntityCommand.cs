using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Commands
{
    public class CreateEntityCommand : IRequest<Entity>
    {
        public string Name { get; set; }
        // Add other properties as needed for entity creation
    }

    public class CreateEntityCommandHandler : IRequestHandler<CreateEntityCommand, Entity>
    {
        private readonly IKafkaProducer _kafkaProducer;

        public CreateEntityCommandHandler(IKafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
        }

        public async Task<Entity> Handle(CreateEntityCommand request, CancellationToken cancellationToken)
        {
            // Create the entity
            var entity = new Entity
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            // Produce a message to Kafka
            await _kafkaProducer.ProduceAsync("entity-topic", entity.Id.ToString(), entity);

            return entity;
        }
    }
}