namespace ODataDemo.Controllers
{
    public class OrderDTO
    {
        public string? Name { get; set; }
        public long Id { get; set; }
        public DateTime? Date { get; set; }
        public long? CustomerId { get; set; }
        public decimal? NetPrice { get; set; }
        public OrderItemDTO[] Items { get; set; }
    }
}
