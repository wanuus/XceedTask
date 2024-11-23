using ApplicationLayer.Dto;

namespace ApplicationLayer.Services.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductDto> UpdateProductAsync(int id, ProductCreateUpdateDto productDto, string userId);
        Task<bool> DeleteProductAsync(int id);
        Task<ProductDto> CreateProduct(ProductCreateUpdateDto productDto, string userId);
        Task<IEnumerable<ProductDto>> GetActiveProductsAsync(DateTime currentTime);
    }
}
