using Application.ApiResult;
using Application.DTOs.Products;
using Application.Interfaces;
using Domain.Entities;
using Domain.Filters;

namespace Application.Services;

public class ProductService(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository)
    : IProductService
{
    public async Task<Result<PagedResult<ProductResponseDto>>> GetPagedAsync(
        ProductFilter filter,
        CancellationToken ct)
    {
        var products = await productRepository.GetPagedAsync(filter, ct);

        var result = new PagedResult<ProductResponseDto>
        {
            Page = products.Page,
            PageSize = products.PageSize,
            TotalCount = products.TotalCount,
            Items = products.Items.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                CreatedByUserId = p.CreatedByUserId
            }).ToList()
        };

        return Result<PagedResult<ProductResponseDto>>.Success(result);
    }

    public async Task<Result<ProductResponseDto>> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(id, ct);

        if (product is null)
        {
            return Result<ProductResponseDto>.Fail(
                "Product not found.",
                ErrorType.NotFound);
        }

        var result = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            CreatedByUserId = product.CreatedByUserId
        };

        return Result<ProductResponseDto>.Success(result);
    }

    public async Task<Result<ProductResponseDto>> CreateAsync(
        CreateProductDto dto,
        int currentUserId,
        CancellationToken ct)
    {
        var validationError = ValidateProduct(
            dto.Name, dto.Price, dto.Stock, dto.Description);

        if (validationError is not null)
        {
            return Result<ProductResponseDto>.Fail(
                validationError, ErrorType.Validation);
        }

        var category = await categoryRepository.GetByIdAsync(
            dto.CategoryId, ct);

        if (category is null)
        {
            return Result<ProductResponseDto>.Fail(
                "Category not found.", ErrorType.Validation);
        }

        var name = dto.Name.Trim();

        var existing = await productRepository.GetByNameInCategoryAsync(
            name, dto.CategoryId, ct);

        if (existing is not null)
        {
            return Result<ProductResponseDto>.Fail(
                "A product with this name already exists in this category.",
                ErrorType.Conflict);
        }

        var product = new Product
        {
            Name = name,
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId,
            CreatedByUserId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await productRepository.CreateAsync(product);

        var result = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            CategoryId = product.CategoryId,
            CategoryName = category.Name,
            CreatedByUserId = product.CreatedByUserId
        };

        return Result<ProductResponseDto>.Success(result);
    }

    public async Task<Result<ProductResponseDto>> UpdateAsync(
        int id,
        UpdateProductDto dto,
        int currentUserId,
        bool isAdmin,
        CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(id, ct);

        if (product is null)
        {
            return Result<ProductResponseDto>.Fail(
                "Product not found.", ErrorType.NotFound);
        }

        if (!isAdmin && product.CreatedByUserId != currentUserId)
        {
            return Result<ProductResponseDto>.Fail(
                "You cannot update another user's product.",
                ErrorType.Forbidden);
        }

        var validationError = ValidateProduct(
            dto.Name, dto.Price, dto.Stock, dto.Description);

        if (validationError is not null)
        {
            return Result<ProductResponseDto>.Fail(
                validationError, ErrorType.Validation);
        }

        var category = await categoryRepository.GetByIdAsync(
            dto.CategoryId, ct);

        if (category is null)
        {
            return Result<ProductResponseDto>.Fail(
                "Category not found.", ErrorType.Validation);
        }

        var name = dto.Name.Trim();

        var existing = await productRepository.GetByNameInCategoryAsync(
            name, dto.CategoryId, ct);

        if (existing is not null && existing.Id != id)
        {
            return Result<ProductResponseDto>.Fail(
                "A product with this name already exists in this category.",
                ErrorType.Conflict);
        }

        product.Name = name;
        product.Description = dto.Description?.Trim();
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;

        await productRepository.UpdateAsync(product);

        var result = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            CategoryId = product.CategoryId,
            CategoryName = category.Name,
            CreatedByUserId = product.CreatedByUserId
        };

        return Result<ProductResponseDto>.Success(result);
    }

    public async Task<Result<bool>> DeleteAsync(
        int id,
        int currentUserId,
        bool isAdmin,
        CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(id, ct);

        if (product is null)
        {
            return Result<bool>.Fail(
                "Product not found.", ErrorType.NotFound);
        }

        if (!isAdmin && product.CreatedByUserId != currentUserId)
        {
            return Result<bool>.Fail(
                "You cannot delete another user's product.",
                ErrorType.Forbidden);
        }

        await productRepository.DeleteAsync(product);

        return Result<bool>.Success(true);
    }

    private static string? ValidateProduct(
        string name,
        decimal price,
        int stock,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            name.Trim().Length < 2 ||
            name.Trim().Length > 200)
        {
            return "Product name must contain 2-200 characters.";
        }

        if (description?.Length > 1000)
        {
            return "Description cannot exceed 1000 characters.";
        }

        if (price <= 0)
        {
            return "Price must be greater than zero.";
        }

        if (stock < 0)
        {
            return "Stock cannot be negative.";
        }

        return null;
    }
}