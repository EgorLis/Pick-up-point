using EgorLis.PickUpPoint.Library.Grpc;
using EgorLis.PickUpPoint.SelfServiceStation.Modules.Webserver.Parts;
using EgorLis.PickUpPoint.SelfServiceStation.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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

    builder.Services.Configure<JsonOptions>(opts =>
    opts.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();   // <-- для minimal API
    builder.Services.AddSwaggerGen(c =>
    {
      c.SwaggerDoc("v1", new()
      {
        Title = "Self-service station API",
        Version = "v1"
      });
      // при необходимости: c.IncludeXmlComments(...);
    });

    // --- Вот это обязательно! Регистрируем regex-constraint ---
    builder.Services.Configure<RouteOptions>(options =>
    {
      options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
    });

    var warehouse = new WarehouseProvider(new Uri("http://localhost:5990"), TimeSpan.FromSeconds(5));

    // Регистрируем зависимости в DI 

    builder.Services
      .AddSingleton(warehouse);

    builder.WebHost.ConfigureKestrel(_opt =>
    {
      _opt.Listen(IPAddress.Any, PortHttp);
    });

    var app = builder.Build();

    // Swagger middleware 
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "Self-service station API V1");
      c.RoutePrefix = "swagger";  // https://localhost:5899/swagger
    });

    var warehouseApi = app.MapGroup("/self-service-station")
                          .WithTags("Self-service station");



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
    })
      .WithName("GetCatalog")
      .WithSummary("Возвращает весь каталог продуктов");


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
    })
      .WithName("GetProduct")
      .WithSummary("Получаем нужный нам продукт");

    return app.RunAsync();
  }
}
