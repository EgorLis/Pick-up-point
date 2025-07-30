using EgorLis.PickUpPoint.Library.Grpc;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;

namespace EgorLis.PickUpPoint.SelfServiceStation.Modules.Webserver.Parts;

public class WarehouseProvider
{
  private Warehouse.WarehouseClient Warehouse { get; set; }
  public bool Available { get; private set; } = false;

  public WarehouseProvider(Uri _warehouseUri, TimeSpan _pingInterval)
  {
    var channel = GrpcChannel.ForAddress(_warehouseUri);
    Warehouse = new Warehouse.WarehouseClient(channel);

    Console.WriteLine($"[WarehouseProvider]|{DateTime.Now:T}| Клиент иницилизирован");

    var stopTaskCts = new CancellationTokenSource();

    DoPingAsync(_pingInterval, stopTaskCts.Token).ConfigureAwait(false);
  }

  public async Task<ProductCatalog> GetProductCatalogAsync(CancellationToken _cancellation) => 
    await Warehouse.GetProductCatalogAsync(new Empty(), cancellationToken: _cancellation).ConfigureAwait(false);

  public async Task<Product> GetProductAsync(GetProductRequest _request, CancellationToken _cancellation) =>
    await Warehouse.GetProductAsync(_request, cancellationToken: _cancellation).ConfigureAwait(false);
  

  private async Task DoPingAsync(TimeSpan _interval, CancellationToken _cancellation)
  {
    while (!_cancellation.IsCancellationRequested)
    {
      try
      {
        await Warehouse.PingAsync(new Empty(), cancellationToken: _cancellation).ConfigureAwait(false);
        Console.WriteLine($"[WarehouseProvider]|{DateTime.Now:T}| Успешный пинг");
        Available = true;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[WarehouseProvider]|{DateTime.Now:T}| Ошибка во время пингования сервиса: {ex.Message}");
        Available = false;
      }

      await Task.Delay(_interval, _cancellation).ConfigureAwait(false);
    }
  }
}
