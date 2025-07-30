using EgorLis.PickUpPoint.Library.Grpc;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace EgorLis.PickUpPoint.Warehouse.Tools;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true,
                             PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
                             WriteIndented = false)]

[JsonSerializable(typeof(ProductCategory))]
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(ProductCatalog))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(GetProductRequest))]
[JsonSerializable(typeof(IEnumerable<Product>))]
[JsonSerializable(typeof(List<Product>))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
public partial class AppJsonSerializerContext : JsonSerializerContext { }
