namespace ODataDemo.Leaders
{
    public class CountryDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long CurrentPresidentId { get; set; }
        public LeaderDTO CurrentPresident { get; set; }
    }
}
