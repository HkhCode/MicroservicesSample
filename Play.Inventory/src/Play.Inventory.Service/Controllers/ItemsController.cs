using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> itemsRepository;
        private readonly IRepository<CatalogItem> CatalogItemsRepository;
        public ItemsController(IRepository<InventoryItem> itemsRepository, IRepository<CatalogItem> CatalogItemsRepository)
        {
            this.itemsRepository = itemsRepository;
            this.CatalogItemsRepository = CatalogItemsRepository;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAsync(Guid UserId)
        {
            if (UserId == Guid.Empty)
            {
                return BadRequest();
            }

            var inventoryItemEntites = await itemsRepository.GetAllAsync(item => item.UserId == UserId);

            var InventoryItemDtos = inventoryItemEntites.Select(inventoryItem =>
            {
                var catalogItem = catalogItems.Single(catalogItem => catalogItem.id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });
            return Ok(InventoryItemDtos);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await itemsRepository.GetAsync(item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);
            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.UserId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };
                await itemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await itemsRepository.UpdateAsync(inventoryItem);
            }
            return Ok();
        }
    }
}