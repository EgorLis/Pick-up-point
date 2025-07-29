using EgorLis.PickUpPoint.Warehouse.Modules.Webserver;

namespace EgorLis.PickUpPoint.Warehouse;

public class Program
{
  public static void Main(string[] args)
  {
    Console.WriteLine("Запуск приложения...");
    Webserver.Run();
  }
}
