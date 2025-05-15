using System.ComponentModel.DataAnnotations;
namespace Tutorial9.Models;

public class ProductWarehouseResponseDTO
{
    public int IdProductWarehouse { get; set; }
    public int IdWarehouse { get; set; }
    public int IdProduct { get; set; }
    public int IdOrder { get; set; }
    public int Amount { get; set; }
    public Decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

//klasa w sumie nieuzyta ostatecznie, ale mozna uzyc gdyby chcialo sie zwracac pelne dane