using ApplicationLayer.Dto;
using ApplicationLayer.Services.Implementations;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Threading.Tasks;
namespace Tests.Services
{
    public class ProductServiceTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<ILogger<ProductService>> _mockLogger;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            // Setup in-memory database

            _context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options);
            _mockLogger = new Mock<ILogger<ProductService>>();
            _productService = new ProductService(_context, _mockLogger.Object);
        }

        private async Task<Product> CreateTestProduct(string name = "Test Product")
        {
            byte[] rand = new byte[5];
            var product = new Product
            {
                Name = name,
                CreationDate = DateTime.UtcNow,
                CreatedByUserId = "testUserId",
                StartDate = DateTime.UtcNow,
                Duration = 7,
                Price = 99.99m,
                CategoryId = 1,
                ImageData = rand,
                ImageMimeType = "ayhaga"

            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        [Fact]
        public async void GetAllProductsAsync_ReturnsAllProducts()
        {
            // Arrange
            await CreateTestProduct("Product 1");
            await CreateTestProduct("Product 2");
            await CreateTestProduct("Product 3");

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Contains(result, p => p.Name == "Product 1");
            Assert.Contains(result, p => p.Name == "Product 2");
            Assert.Contains(result, p => p.Name == "Product 3");
        }

        [Fact]
        public async void GetProductByIdAsync_WithValidId_ReturnsProduct()
        {
            // Arrange
            var product = await CreateTestProduct();

            // Act
            var result = await _productService.GetProductByIdAsync(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal(product.Name, result.Name);
        }

        [Fact]
        public async void GetProductByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _productService.GetProductByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        //[Fact]
//        public async void CreateProduct_CreatesNewProduct()
//        {
//            // Arrange
//            var mockFormFile = new Mock<IFormFile>();
//            var imageBytes = new byte[] { 1, 2, 3 };
//            var stream = new MemoryStream(imageBytes);

//            mockFormFile.Setup(f => f.Length).Returns(imageBytes.Length);
//            mockFormFile.Setup(f => f.ContentType).Returns("image/jpeg");
//            mockFormFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
//                .Callback<Stream, CancellationToken>((stream, token) =>
//                {
//                    stream.Write(imageBytes, 0, imageBytes.Length);
//                })
//                .Returns(Task.FromResult(0); 
//);

//            var productDto = new ProductCreateUpdateDto
//            {
//                Name = "New Product",
//                StartDate = DateTime.UtcNow,
//                Duration = 7,
//                Price = 99.99m,
//                CategoryId = 1,
//                Image = mockFormFile.Object,
//            };

//            // Act
//            var result = await _productService.CreateProduct(productDto, "testUserId");

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(productDto.Name, result.Name);
//            Assert.Equal(productDto.Price, result.Price);
//            Assert.NotNull(result.ImageData);
//            Assert.Equal("image/jpeg", result.ImageMimeType);
//        }

        [Fact]
        public async void UpdateProductAsync_WithValidId_UpdatesProduct()
        {

            // Arrange
            var product = await CreateTestProduct();
            var updateDto = new ProductCreateUpdateDto
            {
                Name = "Updated Product",
                StartDate = DateTime.UtcNow,
                Duration = 14,
                Price = 149.99m,
                CategoryId = 2
            };

            // Act
            var result = await _productService.UpdateProductAsync(product.Id, updateDto, "testUserId");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Price, result.Price);
            Assert.Equal(updateDto.Duration, result.Duration);
        }

        [Fact]
        public async void UpdateProductAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var updateDto = new ProductCreateUpdateDto
            {
                Name = "Updated Product",
                StartDate = DateTime.UtcNow,
                Duration = 14,
                Price = 149.99m,
                CategoryId = 2
            };

            // Act
            var result = await _productService.UpdateProductAsync(999, updateDto, "testUserId");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async void DeleteProductAsync_WithValidId_DeletesProduct()
        {
            // Arrange
            var product = await CreateTestProduct();

            // Act
            var result = await _productService.DeleteProductAsync(product.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await _context.Products.FindAsync(product.Id));
        }

        [Fact]
        public async void DeleteProductAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _productService.DeleteProductAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async void GetActiveProductsAsync_ReturnsOnlyActiveProducts()
        {
            byte[] rand = new byte[5];
            // Arrange
            var currentTime = DateTime.UtcNow;

            // Active product
            var activeProduct = new Product
            {
                Name = "Active Product",
                StartDate = currentTime.AddDays(-1),
                Duration = 3,
                CreatedByUserId = "testUserId",
                ImageData = rand,
                ImageMimeType = "ayhaga"
            };

            // Expired product
            var expiredProduct = new Product
            {
                Name = "Expired Product",
                StartDate = currentTime.AddDays(-5),
                Duration = 2,
                CreatedByUserId = "testUserId",
                ImageData = rand,
                ImageMimeType = "ayhaga"
            };

            // Future product
            var futureProduct = new Product
            {
                Name = "Future Product",
                StartDate = currentTime.AddDays(1),
                Duration = 3,
                CreatedByUserId = "testUserId",
                ImageData = rand,
                ImageMimeType = "ayhaga"
            };

            await _context.Products.AddRangeAsync(activeProduct, expiredProduct, futureProduct);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetActiveProductsAsync(currentTime);

            // Assert
            Assert.Single(result);
            Assert.Contains(result, p => p.Name == "Active Product");
            Assert.DoesNotContain(result, p => p.Name == "Expired Product");
            Assert.DoesNotContain(result, p => p.Name == "Future Product");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}