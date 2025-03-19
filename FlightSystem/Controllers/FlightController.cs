using FlightSystem.Models;
using FlightSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightSystem.Controllers
{
    [Route("api/flights")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly IFlightService _airSialService;
        private readonly IFlightService _alHindService;

        public FlightController(IEnumerable<IFlightService> flightServices)
        {
            //_airSialService = flightServices.OfType<AirSialService>().FirstOrDefault();
            _alHindService = flightServices.OfType<AlHindService>().FirstOrDefault();


            Console.WriteLine($"AirSial Service: {_airSialService}");
            Console.WriteLine($"AlHind Service: {_alHindService}");

        }

        [HttpPost]
        public async Task<IActionResult> PostFlights([FromBody] AirlineRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AirlineName))
            {
                return BadRequest(new { message = "AirlineName is required and must be a valid string." });
            }

            var airlineName = request.AirlineName.Trim().ToLowerInvariant();

            var flights = airlineName switch
            {
                "airsial" => _airSialService != null ? await _airSialService.GetFlightsAsync() : new List<FlightDetails>(),
                "alhind" => _alHindService != null ? await _alHindService.GetFlightsAsync() : new List<FlightDetails>(),
                _ => new List<FlightDetails>()
            };



            if (flights == null || flights.Count == 0)
            {
                return NotFound(new { message = "No flights found for the given airline." });
            }

            return Ok(flights);
        }

    }

    // Request Model for POST API
    public class AirlineRequest
    {
        public string AirlineName { get; set; }
    }
}
