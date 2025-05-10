using Application.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Commands
{
    public class DeleteEntityCommand : IRequest
    {
        public Guid Id { get; set; }
    }

    public class DeleteEntityCommandHandler : IRequestHandler<DeleteEntityCommand>
    {
        private readonly IEntityRepository _repository;

        public DeleteEntityCommandHandler(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(DeleteEntityCommand request, CancellationToken cancellationToken)
        {
            // Delete the entity using the repository
            await _repository.DeleteAsync(request.Id);
            return Unit.Value;
        }
    }
}