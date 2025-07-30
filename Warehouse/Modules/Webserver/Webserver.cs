using EgorLis.PickUpPoint.Library.Grpc;
using EgorLis.PickUpPoint.Warehouse.Modules.AppCore;
using EgorLis.PickUpPoint.Warehouse.Modules.Webserver.Parts;
using EgorLis.PickUpPoint.Warehouse.Tools;
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


    var appCore = new ApplicationCore();

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

    var app = builder.Build();

    var warehouseApi = app.MapGroup("/warehouse");


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
    });


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
    });

    app.MapGrpcService<GrpcService>();

    return app.RunAsync();
  }
}
