using App.Application.Abstractions;
using App.Application.Events;
using App.Modules.Products.Domain;
using App.SharedKernel.Common;
using FluentValidation;
using MediatR;

namespace App.Modules.Products.Application.Commands;

public record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock) : IRequest<Result<Guid>>;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı zorunludur.")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat negatif olamaz.");
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz.");
    }
}

public class CreateProductHandler(
    IRepository<Product> repository,
    IUnitOfWork unitOfWork,
    IPublisher publisher) : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock
        };

        await repository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Diğer modüller (örn. Bildirimler) bu event'i dinleyebilir.
        await publisher.Publish(
            new EntityChangedEvent(nameof(Product), product.Id, "Created", product.Name),
            cancellationToken);

        return Result.Success(product.Id);
    }
}
