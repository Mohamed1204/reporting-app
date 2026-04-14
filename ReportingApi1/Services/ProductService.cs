using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.DTOs;
using ReportingApi1.Entities;

namespace ReportingApi1.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
    }
    public class ProductService : IProductService
    {
        private readonly VatReportingContext _context;

        public ProductService(VatReportingContext context)
        {
            _context = context;
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return null;
            
            return mapToDTO(product);
        }
        public async Task<List<ProductDto>> GetAllAsync()
        {
            return await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                })
                .ToListAsync();
        }

        public static ProductDto mapToDTO(Product product)
        {

            return new ProductDto
            {
                Name = product.Name,
                Category = product.Category,
                Price = product.Price
            };
        }


    }
}
