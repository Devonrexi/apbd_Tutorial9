using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Models;

namespace Tutorial9.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;

    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public async Task<int> AddProductToWarehouseAsync(WarehouseProductDTO dto)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var checkProduct = new SqlCommand("SELECT 1 FROM Product WHERE IdProduct = @id", connection,
                (SqlTransaction)transaction);
            checkProduct.Parameters.AddWithValue("@id", dto.IdProduct);
            if (await checkProduct.ExecuteScalarAsync() is null)
                throw new Exception("Error: Product not found!");
            
            var checkWarehouse = new SqlCommand("SELECT 1 FROM Warehouse WHERE IdWarehouse = @id", connection,
                (SqlTransaction)transaction);
            checkWarehouse.Parameters.AddWithValue("@id", dto.IdWarehouse);
            if (await checkWarehouse.ExecuteScalarAsync() is null)
                throw new Exception("Error: Warehouse not found");
            
            var findOrder = new SqlCommand(@"
                SELECT TOP 1 o.IdOrder, p.Price
                FROM [Order] o
                JOIN Product p ON o.IdProduct = p.IdProduct
                LEFT JOIN Product_Warehouse pw ON o.IdOrder = pw.IdOrder
                WHERE o.IdProduct = @productId AND o.Amount = @amount
                  AND o.CreatedAt < @createdAt
                  AND pw.IdProductWarehouse IS NULL", connection, (SqlTransaction)transaction);

            findOrder.Parameters.AddWithValue("@productId", dto.IdProduct);
            findOrder.Parameters.AddWithValue("@amount", dto.Amount);
            findOrder.Parameters.AddWithValue("@createdAt", dto.CreatedAt);

            int orderId;
            decimal unitPrice;

            await using (var reader = await findOrder.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                    throw new Exception("Error: No valid order found");

                orderId = reader.GetInt32(0);
                unitPrice = reader.GetDecimal(1);
            }
            
            var updateOrder = new SqlCommand("UPDATE [Order] SET FulfilledAt = @date WHERE IdOrder = @id", connection,
                (SqlTransaction)transaction);
            updateOrder.Parameters.AddWithValue("@date", dto.CreatedAt);
            updateOrder.Parameters.AddWithValue("@id", orderId);
            await updateOrder.ExecuteNonQueryAsync();
            
            var insert = new SqlCommand(@"
                INSERT INTO Product_Warehouse 
                (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                OUTPUT INSERTED.IdProductWarehouse
                VALUES (@wid, @pid, @oid, @qty, @price, @created)", connection, (SqlTransaction)transaction);

            insert.Parameters.AddWithValue("@wid", dto.IdWarehouse);
            insert.Parameters.AddWithValue("@pid", dto.IdProduct);
            insert.Parameters.AddWithValue("@oid", orderId);
            insert.Parameters.AddWithValue("@qty", dto.Amount);
            insert.Parameters.AddWithValue("@price", unitPrice * dto.Amount);
            insert.Parameters.AddWithValue("@created", dto.CreatedAt);

            int result = (int)await insert.ExecuteScalarAsync();

            await transaction.CommitAsync();

            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> AddProductToWarehouseProcedureAsync(WarehouseProductDTO dto)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await connection.OpenAsync();

        await using var command = new SqlCommand("AddProductToWarehouse", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@IdProduct", dto.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", dto.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", dto.Amount);
        command.Parameters.AddWithValue("@CreatedAt", dto.CreatedAt);

        var result = await command.ExecuteScalarAsync();
        if (result == null)
            throw new Exception("Error: Procedure did not return inserted ID");

        return Convert.ToInt32(result);
    }
}
