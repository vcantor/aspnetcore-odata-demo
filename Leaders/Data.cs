using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ODataDemo.Leaders
{
    public static class Data
    {
        private const string LeadersPath = @"Leaders\all-leaders.json";

        public static IList<LeaderDTO> Leaders = JsonSerializer.Deserialize<IList<LeaderDTO>>(File.ReadAllText(LeadersPath, Encoding.UTF8), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true

        });
    }
}
