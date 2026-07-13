using App.Application.Abstractions;
using App.Modules.Products.Domain;
using App.SharedKernel.Common;
using MediatR;

namespace App.Modules.Products.Application.Commands;

public record DeleteProductCommand(Guid Id) : IRequest<Result>, ICacheInvalidator
{
    public IEnumerable<string> CacheKeysToInvalidate =>
        [ProductCacheKeys.All, ProductCacheKeys.ById(Id)];
}

public class DeleteProductHandler(
    IRepository<Product> repository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.Failure("Ürün bulunamadı.");

        repository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
