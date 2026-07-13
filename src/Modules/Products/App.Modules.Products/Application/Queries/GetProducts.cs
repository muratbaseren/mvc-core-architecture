using App.Application.Abstractions;
using App.Modules.Products.Domain;
using MediatR;

namespace App.Modules.Products.Application.Queries;

public record GetProductsQuery : IRequest<List<Product>>;

public class GetProductsHandler(IRepository<Product> repository)
    : IRequestHandler<GetProductsQuery, List<Product>>
{
    public Task<List<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        repository.ListAsync(cancellationToken);
}
