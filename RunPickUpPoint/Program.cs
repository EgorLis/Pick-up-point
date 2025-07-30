using System.Diagnostics;

namespace RunPickUpPoint;
class Program
{
  static void Main()
  {
    var baseDir = AppContext.BaseDirectory;
    var warehouseExe = Path.Combine(baseDir, "Warehouse.exe");
    var selfServiceExe = Path.Combine(baseDir, "SelfServiceStation.exe");

    // Проверка наличия файлов
    if (!File.Exists(warehouseExe))
    {
      Console.Error.WriteLine($"Не найден файл: {warehouseExe}");
      return;
    }
    if (!File.Exists(selfServiceExe))
    {
      Console.Error.WriteLine($"Не найден файл: {selfServiceExe}");
      return;
    }

    // Запуск сервисов
    Console.WriteLine("Запуск Warehouse...");
    Process.Start(new ProcessStartInfo(warehouseExe) { UseShellExecute = true });
    Console.WriteLine("Запуск Self-service station...");
    Process.Start(new ProcessStartInfo(selfServiceExe) { UseShellExecute = true });

    // Ожидание
    Console.WriteLine("Ожидание инициализации сервисов...");
    Thread.Sleep(TimeSpan.FromSeconds(5));

    // Открытие Swagger UI
    var warehouseSwaggerUrl = "http://localhost:5999/swagger";
    var selfSwaggerUrl = "http://localhost:5899/swagger";

    Console.WriteLine($"Открытие Swagger Warehouse: {warehouseSwaggerUrl}");
    Process.Start(new ProcessStartInfo(warehouseSwaggerUrl) { UseShellExecute = true });

    Console.WriteLine($"Открытие Swagger Self-service station: {selfSwaggerUrl}");
    Process.Start(new ProcessStartInfo(selfSwaggerUrl) { UseShellExecute = true });

    Console.WriteLine("Готово. Нажмите любую клавишу для выхода...");
    Console.ReadKey();
  }
}
