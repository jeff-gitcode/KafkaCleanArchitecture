using Application.Interfaces;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Queries
{
    public class GetEntityQuery : IRequest<Entity>
    {
        public Guid Id { get; set; }
    }

    public class GetEntityQueryValidator : AbstractValidator<GetEntityQuery>
    {
        public GetEntityQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.")
                .NotEqual(Guid.Empty).WithMessage("Id must be a valid GUID.");
        }
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