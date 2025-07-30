using EgorLis.PickUpPoint.Library.Grpc;
using EgorLis.PickUpPoint.Warehouse.Modules.AppCore;
using EgorLis.PickUpPoint.Warehouse.Modules.Webserver.Parts;
using EgorLis.PickUpPoint.Warehouse.Tools;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;


namespace EgorLis.PickUpPoint.Warehouse.Modules.Webserver;

public static class Webserver
{
  public static int PortGrpc { get; } = 5990;
  public static int PortHttp { get; } = 5999;
  public static Task RunAsync()
  {
    var builder = WebApplication.CreateSlimBuilder();

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
      options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    });

    builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(opts =>
    opts.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

    var appCore = new ApplicationCore();

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();   // <-- для minimal API
    builder.Services.AddSwaggerGen(c =>
    {
      c.SwaggerDoc("v1", new()
      {
        Title = "Warehouse API",
        Version = "v1"
      });
      // при необходимости: c.IncludeXmlComments(...);
    });

    // Регистрируем зависимости в DI 

    builder.Services
      .AddSingleton(appCore)
      .AddSingleton<GrpcService>();

    builder.Services.AddGrpc();

    // Настраиваем порты, для чтения по grpc, http

    builder.WebHost.ConfigureKestrel(_opt =>
    {
      _opt.Listen(IPAddress.Any, PortHttp);
      _opt.Listen(IPAddress.Parse("0.0.0.0"), PortGrpc, options =>
      {
        options.Protocols = HttpProtocols.Http2;
      });
    });

    // --- Вот это обязательно! Регистрируем regex-constraint ---
    builder.Services.Configure<RouteOptions>(options =>
    {
      options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
    });

    var app = builder.Build();

    // Swagger middleware 
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse API V1");
      c.RoutePrefix = "swagger";  // https://localhost:5999/swagger
    });

    var warehouseApi = app.MapGroup("/warehouse")
                          .WithTags("Warehouse");

    // Использованием minimal api 
    warehouseApi.MapGet("/get-catalog", () =>
    {
      try
      {
        return Results.Json(appCore.GetCatalog(), AppJsonSerializerContext.Default.ProductCatalog);
      }
      catch (Exception ex)
      {
        return Results.Problem(detail: ex.Message, statusCode: (int)HttpStatusCode.BadRequest);
      }
    })
      .WithName("GetCatalog")
      .WithSummary("Возвращает весь каталог продуктов");


    warehouseApi.MapPost("/add", (Product _product) =>
    {
      try
      {
        appCore.AddToWarehouse(_product);
        return Results.Ok();
      }
      catch(Exception ex) 
      {
        return Results.Problem(detail: ex.Message, statusCode: (int)HttpStatusCode.BadRequest);
      }
    })
      .WithName("AddProduct")
      .WithSummary("Добавляет новый продукт на склад");

    app.MapGrpcService<GrpcService>();

    return app.RunAsync();
  }
}
