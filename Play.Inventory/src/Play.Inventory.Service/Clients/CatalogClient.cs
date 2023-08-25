using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Play.Inventory.Service.Dtos;

namespace Play.Inventory.Service.Clients
{
    public class CatalogClient
    {
        private readonly HttpClient client;
        public CatalogClient(HttpClient client)
        {
            this.client = client;
        }

        public async Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemsAsync()
        {
            var items = await client.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("/items");
            return items;
        }
    }
}