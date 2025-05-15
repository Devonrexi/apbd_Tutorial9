using Tutorial9.Models;
namespace Tutorial9.Services;

public interface IDbService
{
    Task<int> AddProductToWarehouseAsync(WarehouseProductDTO dto);
    Task<int> AddProductToWarehouseProcedureAsync(WarehouseProductDTO dto);
}