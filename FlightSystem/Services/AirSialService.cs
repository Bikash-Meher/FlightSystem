using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FlightSystem.Models;

namespace FlightSystem.Services
{
    public class AirSialService : IAirSialFlightServices
    {
        private readonly string _airSialPath = "AirsialResponse.json";

        public async Task<ApiAirSialResponse?> GetFlightsAsync()
        {
            var rawResponse = await GetRawFlightsAsync();
            return rawResponse != null ? AirSialMapper.MapToCommonResponse(rawResponse) : null;
        }

        private async Task<ApiAirSialResponse?> GetRawFlightsAsync()
        {
            if (!File.Exists(_airSialPath))
            {
                return null;
            }

            await using var stream = File.OpenRead(_airSialPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await JsonSerializer.DeserializeAsync<ApiAirSialResponse>(stream, options);
        }
    }
}
