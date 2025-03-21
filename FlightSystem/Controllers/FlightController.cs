using FlightSystem.Models;
using FlightSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly IAirSialFlightServices _airSialService;
        private readonly IAlHindFlightService _alHindService;

        public FlightController(IAirSialFlightServices airSialService, IAlHindFlightService alHindService)
        {
            _airSialService = airSialService;
            _alHindService = alHindService;
        }

        [HttpPost("SearchFight")]
        public async Task<IActionResult> GetFlight([FromBody] AirLine airline)
        {
            if (airline == null || string.IsNullOrEmpty(airline.airlineName))
            {
                return BadRequest();
            }

            List<object> flights = new();

            switch (airline.airlineName.ToLower())
            {
                case "airsial":
                    var airSialFlights = await _airSialService.GetFlightsAsync();
                    flights.AddRange(airSialFlights);
                    break;

                case "alhind":
                    var alHindFlights = await _alHindService.GetFlightsAsync();
                    flights.AddRange(alHindFlights);
                    break;

                default:
                    return BadRequest();
            }

            if (!flights.Any())
            {
                return NotFound();
            }

            return Ok(flights);
        }
    }

    public class AirLine
    {
        public string airlineName { get; set; }
    }
}
