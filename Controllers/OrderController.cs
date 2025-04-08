using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Text.Json;

namespace ODataDemo.Controllers
{
    [ApiController]
    public class OrderController : ODataController
    {

        public OrderController(ILogger<OrderController> logger)
        {
            Logger = logger;
        }

        public ILogger<OrderController> Logger { get; }

        private static Task<IEnumerable<OrderDTO>> GetData()
        {
            return Task.FromResult(Enumerable.Range(1, 20).Select(id => new OrderDTO()
            {
                CustomerId = id,
                Id = id,
                Date = DateTime.UtcNow.AddDays(id),
                Name = "test",
                NetPrice = Convert.ToDecimal(Random.Shared.NextDouble()),
                Items = Enumerable.Range(1, 20).Select(itemId => new OrderItemDTO()
                {
                    Id = itemId,
                    OrderId = id,
                    Price = Convert.ToDecimal(Random.Shared.NextDouble())
                }).ToArray()
            }));
        }

        [HttpGet("v1/Orders")]
        [EnableQuery(PageSize = 5)]
        public async Task<IEnumerable<OrderDTO>> GetAll()
        {
            return await GetData();
        }



        [HttpPatch("v1/Orders/{orderId}")]
        [EnableQuery]
        public async Task<IEnumerable<OrderDTO>> PatchOrder([FromRoute] long orderId, [FromBody] Delta<OrderDTO> patchedOrder)
        {
            if (patchedOrder.TryGetPropertyValue(nameof(OrderDTO.Items), out var items) && items is DeltaSet<OrderItemDTO> updatedItems)
            {
                if (ModelState.IsValid)
                {

                }
                Logger.LogInformation("Patched Instance for orderid {orderId} is {@patched}", orderId, JsonSerializer.Serialize(updatedItems));
            }
            Logger.LogInformation("Patched Instance for orderid {orderId} is {@patched}", orderId, JsonSerializer.Serialize(patchedOrder.GetInstance()));
            return await Task.FromResult(new List<OrderDTO>() { patchedOrder.GetInstance() });
        }


        [HttpPatch("v1/Orders/{orderId}/items")]
        [EnableQuery]
        public async void PatchOrderItems([FromRoute] long orderId, [FromBody] DeltaSet<OrderItemDTO> patchedOrder)
        {
            foreach (var item in patchedOrder)
            {
                if (item.Kind == DeltaItemKind.DeletedResource && item is DeltaDeletedResource<OrderItemDTO> deletedItem)
                {
                    Logger.LogInformation("Deleted Instance for orderid {orderId} is {@patched}", orderId, JsonSerializer.Serialize(deletedItem.GetInstance()));
                }
                else if (item.Kind == DeltaItemKind.Resource && item is Delta<OrderItemDTO> upsert)
                {
                    Logger.LogInformation("Patched Instance for orderid {orderId} is {@patched}", orderId, JsonSerializer.Serialize(upsert.GetInstance()));
                }
            }

        }
    }
}
