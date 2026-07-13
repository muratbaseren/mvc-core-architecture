using App.Application.Abstractions;
using App.Modules.Products.Domain;
using MediatR;

namespace App.Modules.Products.Application.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<Product?>;

public class GetProductByIdHandler(IRepository<Product> repository)
    : IRequestHandler<GetProductByIdQuery, Product?>
{
    public Task<Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(request.Id, cancellationToken);
}
