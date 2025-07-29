using EgorLis.PickUpPoint.Library.Grpc;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace EgorLis.PickUpPoint.Warehouse.Tools;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true,
                             PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                             WriteIndented = false)]

[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(ProductCatalog))]
[JsonSerializable(typeof(ProductCategory))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }
