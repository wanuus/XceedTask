namespace ApplicationLayer.Dto
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageMimeType { get; set; }
    }
}
