using EgorLis.PickUpPoint.Library.Grpc;

namespace EgorLis.PickUpPoint.Warehouse.Modules.Catalog;

public class AppCore
{
  public Dictionary<int, Product> Products { get; private set; }
  public AppCore()
  {
    // Иницилизируем коллекцию с изначальными значениями

    Products = new Dictionary<int, Product>
    {
      { 0, new Product { Id = 0, Category = ProductCategory.ComputerParts, Name = "RTX 3080", Count = 5 } },
      { 1, new Product { Id = 1, Category = ProductCategory.ComputerParts, Name = "RTX 3070", Count = 10 } },
      { 2, new Product { Id = 2, Category = ProductCategory.ComputerParts, Name = "RTX 3060", Count = 15 } }
    };

    Console.WriteLine($"Ядро сервиса иницилизированно");
  }

  // Оборачиваем наши данные, для последующий передачи по Grpc
  public ProductCatalog GetCatalog()
  {
    var catalog = new ProductCatalog();
    catalog.Products.AddRange(Products.Values);

    return catalog;
  }


  // Убираем (выдаем) товар со склада
  public Product RemoveFromWarehouse(GetProductRequest _request)
  {
    if (!Products.TryGetValue(_request.ProductId, out var product))
      throw new KeyNotFoundException($"Данного товара id:{_request.ProductId} нет на складе");

    Console.WriteLine($"Товар id:{_request.ProductId} найден");

    if (_request.Count > product.Count)
    {
      var str = $"В наличии нет столько товара id:{_request.ProductId}";
      Console.WriteLine(str);

      throw new ArgumentOutOfRangeException(str);
    }

    // возвращаемое значение
    var resultProduct = new Product(product);
    resultProduct.Count = _request.Count;

    if(_request.Count == product.Count)
    {
      Products.Remove(_request.ProductId);
    }
    else
    {
      product.Count -= _request.Count;
    }

    return product;
  }

  // Добавление товаров на склад
  public void AddToWarehouse(Product _product)
  {
    if (Products.TryGetValue(_product.Id, out var product))
    {
      if (product.Name != _product.Name)
        throw new InvalidOperationException($"Товар не соответсвует уже существующему с таким id:{_product.Id} товару");

      Console.WriteLine($"Найден аналогичный товар id:{_product.Id}, со схожими параметрами, просто суммируем значение");

      product.Count += _product.Count;

      return;
    }

    Products.Add(_product.Id, _product); 
  }
}
