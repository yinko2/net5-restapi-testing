using catalog.api.DTOS;
using catalog.api.Entities;

namespace catalog.api
{
    public static class Extensions
    {
        public static ItemDTO AsDTO(this Item item)
        {
            // return new ItemDTO
            // {
            //     Id = item.Id,
            //     Name = item.Name,
            //     Price = item.Price,
            //     CreatedDate = item.CreatedDate
            // };
            return new ItemDTO
            (
                item.Id,
                item.Name,
                item.Description,
                item.Price,
                item.CreatedDate
            );
        }
    }
}