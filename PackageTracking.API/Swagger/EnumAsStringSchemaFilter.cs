using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PackageTracking.API.Swagger
{
    /// <summary>
    /// Documents enums as JSON strings to match <see cref="System.Text.Json.Serialization.JsonStringEnumConverter"/> at runtime.
    /// </summary>
    public sealed class EnumAsStringSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var enumType = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
            if (!enumType.IsEnum)
                return;

            schema.Type = "string";
            schema.Format = null;
            schema.Enum ??= [];
            schema.Enum.Clear();
            foreach (var name in Enum.GetNames(enumType))
                schema.Enum.Add(new OpenApiString(name));
        }
    }
}
