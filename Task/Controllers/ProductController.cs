using ApplicationLayer.Dto;
using ApplicationLayer.Services.Interface;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            UserManager<ApplicationUser> userManager,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _userManager = userManager;
            _logger = logger;
        }

        private async Task<(string userId, ApplicationUser user)> GetUserFromToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token");
                return (null, null);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return (userId, null);
            }

            return (userId, user);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] ProductCreateUpdateDto productDto)
        {
            try
            {
                var (userId, user) = await GetUserFromToken();
                if (user == null)
                    return Unauthorized("Invalid user");

                var createdProduct = await _productService.CreateProduct(productDto, userId);
                return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromForm] ProductCreateUpdateDto productDto)
        {
            try
            {
                var (userId, user) = await GetUserFromToken();
                if (user == null)
                    return Unauthorized("Invalid user");

                var updatedProduct = await _productService.UpdateProductAsync(id, productDto, userId);
                if (updatedProduct == null)
                    return NotFound();

                return Ok(updatedProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
                throw;
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetActiveProducts()
        {
            var currentTime = DateTime.UtcNow;
            var activeProducts = await _productService.GetActiveProductsAsync(currentTime);
            return Ok(activeProducts);
        }
    }
}
