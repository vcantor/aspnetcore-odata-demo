using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataDemo.Controllers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ODataDemo.Leaders
{

    [ApiController]
    public class LeadersController : ODataController
    {
        public LeadersController(ILogger<LeadersController> logger)
        {
            Logger = logger;
        }

        public ILogger<LeadersController> Logger { get; }

        private static readonly Dictionary<long, LeaderDTO> _leaders = Data.Leaders.ToDictionary(leader => leader.Id);

        private static Dictionary<long, CountryDTO> _countries = Enumerable.Range(1, 20).Select(id => new CountryDTO()
        {
            Id = id,
            Name = Faker.Country.Name(),
            CurrentPresidentId = id,
            CurrentPresident = _leaders[id]
        }).ToDictionary(country => country.Id);


        [HttpGet("v1/leaders")]
        [EnableQuery]
        public async Task<IEnumerable<LeaderDTO>> GetAll()
        {
            return await Task.FromResult(_leaders.Values);
        }

        //[HttpGet("v1/leaders")]
        //[EnableQuery]
        //public async Task<IQueryable<LeaderDTO>> GetAll()
        //{
        //    return await leadersFacade.GetAll();
        //}

        [HttpGet("v1/countries")]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All)]
        public async Task<IEnumerable<CountryDTO>> GetCountries()
        {
            return await Task.FromResult(_countries.Values);
        }
    }
}