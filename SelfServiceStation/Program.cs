using EgorLis.PickUpPoint.SelfServiceStation.Modules.Webserver;

namespace EgorLis.PickUpPoint.SelfServiceStation;

public class Program
{
  public static async Task Main(string[] args)
  {
    Console.WriteLine($"[Main]|{DateTime.Now:T}|Запуск приложения…");

    // Стартуем веб-сервер асинхронно
    var webServerTask = Webserver.RunAsync();

    // Если нужно дождаться завершения сервера (например, по Ctrl+C)
    await webServerTask;
  }
}
