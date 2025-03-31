using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FlightSystem.Models;

namespace FlightSystem.Services
{
    public class AirSialService : IAirSialFlightServices
    {
        private readonly string _airSialPath = "AirsialResponse.json";

        public async Task<ApiAirSialResponse> GetFlightsAsync()
        {
            if (!File.Exists(_airSialPath))
            {
                return new ApiAirSialResponse { Response = new List<FlightBounding>(), Success = false };
            }

            await using var stream = File.OpenRead(_airSialPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var rawResponse = await JsonSerializer.DeserializeAsync<AirSialResponse>(stream, options);

            return rawResponse != null ? AirSialMapping.AirSialFlights(rawResponse) : new ApiAirSialResponse { Response = new List<FlightBounding>(), Success = false };
        }
    }
}
