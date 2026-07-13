namespace App.Web.Infrastructure.GraphQl;

public static class GraphQlNaming
{
    /// <summary>Entity adından camelCase çoğul alan adı üretir: Product -> products, Category -> categories.</summary>
    public static string ToCollectionFieldName(string entityName)
    {
        var camel = char.ToLowerInvariant(entityName[0]) + entityName[1..];
        if (camel.EndsWith('y') && camel.Length > 1 && !"aeiou".Contains(camel[^2]))
            return camel[..^1] + "ies";
        if (camel.EndsWith('s') || camel.EndsWith("ch") || camel.EndsWith("sh") || camel.EndsWith('x') || camel.EndsWith('z'))
            return camel + "es";
        return camel + "s";
    }

    /// <summary>Tekil "id ile getir" alan adı üretir: Product -> productById.</summary>
    public static string ToByIdFieldName(string entityName) =>
        char.ToLowerInvariant(entityName[0]) + entityName[1..] + "ById";
}
