using ApplicationLayer.Dto;
using ApplicationLayer.Services.Interface;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PresentationLayer.Controllers;
using System.Security.Claims;
using Xunit;

namespace Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ILogger<ProductController>> _mockLogger;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            
            // Setup mock UserManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            
            _mockLogger = new Mock<ILogger<ProductController>>();
            
            _controller = new ProductController(
                _mockProductService.Object,
                _mockUserManager.Object,
                _mockLogger.Object);

            // Setup default user context
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "testUserId")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async void GetAllProducts_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange
            var expectedProducts = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Test Product 1" },
                new ProductDto { Id = 2, Name = "Test Product 2" }
            };
            _mockProductService.Setup(service => service.GetAllProductsAsync())
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await _controller.GetAllProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
            Assert.Equal(expectedProducts.Count, returnedProducts.Count());
        }

        [Fact]
        public async void GetProduct_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var productId = 1;
            var expectedProduct = new ProductDto { Id = productId, Name = "Test Product" };
            _mockProductService.Setup(service => service.GetProductByIdAsync(productId))
                .ReturnsAsync(expectedProduct);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal(expectedProduct.Id, returnedProduct.Id);
        }

        [Fact]
        public async void GetProduct_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = 999;
            _mockProductService.Setup(service => service.GetProductByIdAsync(productId))
                .ReturnsAsync((ProductDto)null);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void CreateProduct_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var productDto = new ProductCreateUpdateDto { Name = "New Product" };
            var createdProduct = new ProductDto { Id = 1, Name = "New Product" };
            
            _mockUserManager.Setup(x => x.FindByIdAsync("testUserId"))
                .ReturnsAsync(new ApplicationUser { Id = "testUserId" });
            
            _mockProductService.Setup(service => service.CreateProduct(productDto, "testUserId"))
                .ReturnsAsync(createdProduct);

            // Act
            var result = await _controller.CreateProduct(productDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedProduct = Assert.IsType<ProductDto>(createdAtActionResult.Value);
            Assert.Equal(createdProduct.Id, returnedProduct.Id);
        }

        [Fact]
        public async void DeleteProduct_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var productId = 1;
            _mockProductService.Setup(service => service.DeleteProductAsync(productId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProduct(productId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void DeleteProduct_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = 999;
            _mockProductService.Setup(service => service.DeleteProductAsync(productId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteProduct(productId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void GetActiveProducts_ReturnsOkResult_WithActiveProducts()
        {
            // Arrange
            var expectedProducts = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Active Product 1" },
                new ProductDto { Id = 2, Name = "Active Product 2" }
            };
            _mockProductService.Setup(service => service.GetActiveProductsAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await _controller.GetActiveProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
            Assert.Equal(expectedProducts.Count, returnedProducts.Count());
        }
    }
} 