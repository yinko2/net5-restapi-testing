using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using catalog.api.DTOS;
using catalog.api.Entities;
using catalog.api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace catalog.api.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemsRepository repository;
        private readonly ILogger<ItemsController> logger;

        public ItemsController(IItemsRepository _repository, ILogger<ItemsController> _logger)
        {
            this.repository = _repository;
            this.logger = _logger;
        }

        // GET /items
        [HttpGet]
        public async Task<IEnumerable<ItemDTO>> GetItemsAsync(string name = null)
        {
            var items = (await repository.GetItemsAsync()).Select( item => item.AsDTO() );

            if (!string.IsNullOrWhiteSpace(name))
            {
                items = items.Where(item => item.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            logger.LogInformation($"{DateTime.UtcNow.ToString()}: Retrieved {items.Count()} Items");

            return items;
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDTO>> GetItemAsync(Guid id)
        {
            var item = await repository.GetItemAsync(id);
            
            if (item is null)
            {
                return NotFound();
            }

            return Ok(item.AsDTO());
        }

        //POST /items
        [HttpPost]
        public async Task<ActionResult<ItemDTO>> CreateItemAsync(CreateItemDTO itemDTO)
        {
            Item item = new(){
                Id = Guid.NewGuid(),
                Name = itemDTO.Name,
                Description = itemDTO.Description,
                Price = itemDTO.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await repository.CreateItemAsync(item);

            return CreatedAtAction(nameof(GetItemAsync), new { id = item.Id }, item.AsDTO());
        }

        //PUT /items/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateItemAsync(Guid id, UpdateItemDTO itemDTO)
        {
            var existingItem = await repository.GetItemAsync(id);

            if (existingItem is null)
            {
                return NotFound();
            }

            existingItem.Name = itemDTO.Name;
            existingItem.Price = itemDTO.Price;

            // Item updatedItem = existingItem with {
            //     Name = itemDTO.Name,
            //     Price = itemDTO.Price
            // };

            await repository.UpdateItemAsync(existingItem);

            return NoContent();
        }

        //DELETE /items/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItemAsync(Guid id)
        {
            var existingItem = await repository.GetItemAsync(id);

            if (existingItem is null)
            {
                return NotFound();
            }

            await repository.DeleteItemAsync(id);
            
            return NoContent();
        }
    }
}