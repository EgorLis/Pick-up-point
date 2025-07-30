using EgorLis.PickUpPoint.Warehouse.Modules.AppCore;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using EgorLis.PickUpPoint.Library.Grpc;

namespace EgorLis.PickUpPoint.Warehouse.Modules.Webserver.Parts;
public class GrpcService : Library.Grpc.Warehouse.WarehouseBase
{
  private ApplicationCore AppCore { get; init; }
  public GrpcService(ApplicationCore _appCore)
  {
    AppCore = _appCore;

    Console.WriteLine($"[GrpcService]|{DateTime.Now:T}|Grpc сервис иницилизирован");
  }

  public override Task<Product> GetProduct(GetProductRequest _request, ServerCallContext _context)
  {
    try
    {
      return Task.FromResult(AppCore.RemoveFromWarehouse(_request));
    }
    catch(Exception ex)
    {
      throw new RpcException(new Status(StatusCode.Internal, $"Internal server error: {ex.Message}"));
    }
  }

  public override Task<ProductCatalog> GetProductCatalog(Empty _request, ServerCallContext _context)
  {
    try
    {
      return Task.FromResult(AppCore.GetCatalog());
    }
    catch (Exception ex)
    {
      throw new RpcException(new Status(StatusCode.Internal, $"Internal server error: {ex.Message}"));
    }
  }

  public override Task<Empty> Ping(Empty _request, ServerCallContext _context)
  {
    return Task.FromResult(new Empty());
  }
}
