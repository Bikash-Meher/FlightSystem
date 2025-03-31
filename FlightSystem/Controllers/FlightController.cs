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

        [HttpPost("SearchFlight")]
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
                    var airSialResponse = await _airSialService.GetFlightsAsync();
                    if (airSialResponse.Success)
                    {
                        flights.AddRange(airSialResponse.Response);
                    }
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
        public required string airlineName { get; set; }
    }
}
