using EgorLis.PickUpPoint.Library.Grpc;
using EgorLis.PickUpPoint.SelfServiceStation.Modules.Webserver.Parts;
using EgorLis.PickUpPoint.SelfServiceStation.Tools;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EgorLis.PickUpPoint.SelfServiceStation.Modules.Webserver;

public static class Webserver
{
  public static int PortHttp { get; } = 5899;
  public static Task RunAsync()
  {
    var builder = WebApplication.CreateSlimBuilder();

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
      options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    });


    var warehouse = new WarehouseProvider(new Uri("http://localhost:5990"), TimeSpan.FromSeconds(5));

    // Регистрируем зависимости в DI 

    builder.Services
      .AddSingleton(warehouse);

    var app = builder.Build();

    var warehouseApi = app.MapGroup("/self-service-station");


    // Использованием minimal api 
    warehouseApi.MapGet("/get-catalog", async () =>
    {
      if(!warehouse.Available)
        return Results.Problem(detail: "Склад недоступен", statusCode: (int)HttpStatusCode.ServiceUnavailable);

      try
      {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        return Results.Json(await warehouse.GetProductCatalogAsync(cts.Token), AppJsonSerializerContext.Default.ProductCatalog);
      }
      catch (Exception ex)
      {
        return Results.Problem(detail: ex.Message, statusCode: (int)HttpStatusCode.BadRequest);
      }
    });


    warehouseApi.MapGet("/get-product", async
      ([FromQuery(Name = "id")] int _id, 
      [FromQuery(Name = "count")] int _count) =>
    {
      if (!warehouse.Available)
        return Results.Problem(detail: "Склад недоступен", statusCode: (int)HttpStatusCode.ServiceUnavailable);

      try
      {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var request = new GetProductRequest() { Count = _count, ProductId = _id };
        var product = await warehouse.GetProductAsync(request, cts.Token).ConfigureAwait(false);
        return Results.Json(product, AppJsonSerializerContext.Default.Product);
      }
      catch(Exception ex) 
      {
        return Results.Problem(detail: ex.Message, statusCode: (int)HttpStatusCode.BadRequest);
      }
    });

    return app.RunAsync();
  }
}
