using App.Infrastructure.Persistence;
using App.SharedKernel.Domain;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace App.Web.Infrastructure.GraphQl;

/// <summary>
/// Her aggregate root entity için GraphQL Query tipine otomatik alanlar ekler:
/// <c>products(filtre/sıralama/sayfalama)</c> ve <c>productById(id)</c> gibi.
/// Yeni bir modül entity'si eklendiğinde şemaya elle dokunmak gerekmez.
/// </summary>
public class EntityQueryExtension<TEntity> : ObjectTypeExtension
    where TEntity : BaseEntity
{
    private const string DbContextKey = "AppDbContextInstance";

    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name(OperationTypeNames.Query);

        var entityName = typeof(TEntity).Name;

        // Liste alanı: sayfalama + filtreleme + sıralama destekli.
        // En dıştaki middleware, sorgu materyalize edilene kadar yaşayacak
        // bir DbContext örneğini factory'den üretir ve iş bitince dispose eder.
        descriptor
            .Field(GraphQlNaming.ToCollectionFieldName(entityName))
            .Use(next => async ctx =>
            {
                await using var db = await ctx
                    .Service<IDbContextFactory<AppDbContext>>()
                    .CreateDbContextAsync(ctx.RequestAborted);
                ctx.SetLocalState(DbContextKey, db);
                await next(ctx);
            })
            .UsePaging()
            .UseFiltering()
            .UseSorting()
            .Resolve(ctx => ctx
                .GetLocalState<AppDbContext>(DbContextKey)!
                .Set<TEntity>()
                .AsNoTracking());

        // Tekil alan: id ile getir.
        descriptor
            .Field(GraphQlNaming.ToByIdFieldName(entityName))
            .Type<ObjectType<TEntity>>()
            .Argument("id", a => a.Type<NonNullType<UuidType>>())
            .Resolve(async ctx =>
            {
                var id = ctx.ArgumentValue<Guid>("id");
                await using var db = await ctx
                    .Service<IDbContextFactory<AppDbContext>>()
                    .CreateDbContextAsync(ctx.RequestAborted);
                return await db.Set<TEntity>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id, ctx.RequestAborted);
            });
    }
}
