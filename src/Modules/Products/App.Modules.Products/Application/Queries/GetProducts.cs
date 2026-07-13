using App.Application.Abstractions;
using App.Modules.Products.Domain;
using MediatR;

namespace App.Modules.Products.Application.Queries;

/// <summary>ICacheableQuery: sonuç 60 sn önbellekte tutulur; ürün komutları temizler.</summary>
public record GetProductsQuery : IRequest<List<Product>>, ICacheableQuery
{
    public string CacheKey => ProductCacheKeys.All;
}

public class GetProductsHandler(IRepository<Product> repository)
    : IRequestHandler<GetProductsQuery, List<Product>>
{
    public Task<List<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        repository.ListAsync(cancellationToken);
}
