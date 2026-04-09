using Microsoft.EntityFrameworkCore;
using ReportingApi1.Data;
using ReportingApi1.DTOs;

namespace ReportingApi1.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();
    }
    public class ProductService : IProductService
    {
        private readonly VatReportingContext _context;

        public ProductService(VatReportingContext context)
        {
            _context = context;
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
    }
}
