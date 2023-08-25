using System;

namespace Play.Catalog.Contracts
{
    public record CatalogItemCreated(Guid itemId, string Name, string Description);
    public record CatalogItemUpdated(Guid itemId, string Name, string Description);
    public record CatalogItemDeleted(Guid itemId);
}