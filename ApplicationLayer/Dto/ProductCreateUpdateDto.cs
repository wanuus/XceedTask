using Microsoft.AspNetCore.Http;

namespace ApplicationLayer.Dto
{
    public class ProductCreateUpdateDto
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public IFormFile Image { get; set; }
    }
}