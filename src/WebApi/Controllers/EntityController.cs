using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Commands;
using Application.Queries;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntityController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EntityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntity([FromBody] CreateEntityCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetEntity), new { id = result?.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntity(Guid id)
        {
            var query = new GetEntityQuery { Id = id };
            var entity = await _mediator.Send(query);

            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEntities()
        {
            var query = new GetAllEntitiesQuery();
            var entities = await _mediator.Send(query);
            return Ok(entities);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntity(Guid id)
        {
            var command = new DeleteEntityCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}