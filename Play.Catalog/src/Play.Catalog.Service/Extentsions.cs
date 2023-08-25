using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entites;

namespace Play.Catalog.Service
{
    public static class Extentions
    {
        public static ItemDto AsDto(this item entity)
        {
            return new ItemDto(entity.Id, entity.Name, entity.Description, entity.Price, entity.CreatedDate);
        }
    }
}