using Microsoft.AspNetCore.Mvc;
using Tutorial9.Models;
    using Tutorial9.Services;
namespace Tutorial9.Controllers;

    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IDbService _service;
 
        public WarehouseController(IDbService service)
        {
            _service = service;
        }
 
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] WarehouseProductDTO dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            try
            {
                var newId = await _service.AddProductToWarehouseAsync(dto);
                return Ok(newId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("procedure")]
        public async Task<IActionResult> AddProductToWarehouseProcedureAsync([FromBody] WarehouseProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
 
            try
            {
                var result = await _service.AddProductToWarehouseProcedureAsync(dto);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }