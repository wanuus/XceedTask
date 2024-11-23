using ApplicationLayer.Dto;
using ApplicationLayer.Services.Interface;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            AppDbContext context,
            ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task<(byte[] imageData, string mimeType)> ProcessImageFile(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return (null, null);

            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            return (memoryStream.ToArray(), image.ContentType);
        }

        public async Task<ProductDto> CreateProduct(ProductCreateUpdateDto productDto, string userId)
        {
            _logger.LogInformation("User {UserId} is creating new product with name: {ProductName}",
                userId, productDto.Name);

            var (imageData, mimeType) = await ProcessImageFile(productDto.Image);

            var product = new Product
            {
                Name = productDto.Name,
                CreationDate = DateTime.UtcNow,
                CreatedByUserId = userId,
                StartDate = productDto.StartDate,
                Duration = productDto.Duration,
                Price = productDto.Price,
                CategoryId = productDto.CategoryId,
                ImageData = imageData,
                ImageMimeType = mimeType
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} successfully created product with ID: {ProductId}",
                userId, product.Id);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                CreationDate = product.CreationDate,
                CreatedByUserId = product.CreatedByUserId,
                StartDate = product.StartDate,
                Duration = product.Duration,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ImageData = product.ImageData,
                ImageMimeType = product.ImageMimeType
            };
        }

        public async Task<ProductDto> UpdateProductAsync(int id, ProductCreateUpdateDto productDto, string userId)
        {
            _logger.LogInformation("User {UserId} is updating product with ID: {ProductId}", userId, id);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("User {UserId} attempted to update non-existent product with ID: {ProductId}",
                    userId, id);
                return null;
            }

            if (productDto.Image != null)
            {
                var (imageData, mimeType) = await ProcessImageFile(productDto.Image);
                product.ImageData = imageData;
                product.ImageMimeType = mimeType;
            }

            product.Name = productDto.Name;
            product.StartDate = productDto.StartDate;
            product.Duration = productDto.Duration;
            product.Price = productDto.Price;
            product.CategoryId = productDto.CategoryId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} successfully updated product with ID: {ProductId} at {Time}", userId, id, DateTime.Now);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                CreationDate = product.CreationDate,
                CreatedByUserId = product.CreatedByUserId,
                StartDate = product.StartDate,
                Duration = product.Duration,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ImageData = product.ImageData,
                ImageMimeType = product.ImageMimeType
            };
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            _logger.LogInformation("Retrieving all products");

            var products = await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CreationDate = p.CreationDate,
                    CreatedByUserId = p.CreatedByUserId,
                    StartDate = p.StartDate,
                    Duration = p.Duration,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    ImageData = p.ImageData,
                    ImageMimeType = p.ImageMimeType
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} products", products.Count);
            return products;
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving product with ID: {ProductId}", id);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Attempted to retrieve non-existent product with ID: {ProductId}", id);
                return null;
            }

            _logger.LogInformation("Successfully retrieved product with ID: {ProductId}", id);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                CreationDate = product.CreationDate,
                CreatedByUserId = product.CreatedByUserId,
                StartDate = product.StartDate,
                Duration = product.Duration,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ImageData = product.ImageData,
                ImageMimeType = product.ImageMimeType
            };
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            _logger.LogInformation("Attempting to delete product with ID: {ProductId}", id);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Attempted to delete non-existent product with ID: {ProductId}", id);
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted product with ID: {ProductId}", id);
            return true;
        }

        public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync(DateTime currentTime)
        {
            _logger.LogInformation("Fetching active products for time: {CurrentTime}", currentTime);

            var products = await _context.Products
                .Where(p => p.StartDate <= currentTime &&
                           p.StartDate.AddDays(p.Duration) >= currentTime)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CreationDate = p.CreationDate,
                    CreatedByUserId = p.CreatedByUserId,
                    StartDate = p.StartDate,
                    Duration = p.Duration,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    ImageData = p.ImageData,
                    ImageMimeType = p.ImageMimeType
                })
                .ToListAsync();

            _logger.LogInformation("Found {Count} active products", products.Count());
            return products;
        }
    }
}
