using App.Modules.Products.Application.Commands;
using HotChocolate;
using HotChocolate.Types;
using MediatR;

namespace App.Modules.Products.GraphQl;

/// <summary>
/// Modüle özel GraphQL mutation örneği. Web katmanı, modül
/// assembly'lerindeki ObjectTypeExtension sınıflarını otomatik keşfeder;
/// bu tip şemaya kendiliğinden eklenir.
/// </summary>
public class ProductMutations : ObjectTypeExtension
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name(OperationTypeNames.Mutation);

        descriptor
            .Field("createProduct")
            .Type<NonNullType<UuidType>>()
            .Argument("name", a => a.Type<NonNullType<StringType>>())
            .Argument("description", a => a.Type<StringType>())
            .Argument("price", a => a.Type<NonNullType<DecimalType>>())
            .Argument("stock", a => a.Type<NonNullType<IntType>>())
            .Resolve(async ctx =>
            {
                var sender = ctx.Service<ISender>();
                var result = await sender.Send(
                    new CreateProductCommand(
                        ctx.ArgumentValue<string>("name"),
                        ctx.ArgumentValue<string?>("description"),
                        ctx.ArgumentValue<decimal>("price"),
                        ctx.ArgumentValue<int>("stock")),
                    ctx.RequestAborted);

                if (!result.IsSuccess)
                    throw new GraphQLException(result.Error!);

                return result.Value;
            });
    }
}
