namespace ODataDemo.Leaders
{
    public class ElectionDTO
    {
        public long Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long LeaderId { get; set; }
        public long CountryId { get; set; }

        public LeaderDTO Candidate { get; set; }
        public CountryDTO Country { get; set; }
    }
}
