using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using FlightSystem.Models;

namespace FlightSystem.Services
{
    public class AlHindService : IAlHindFlightService
    {
        private readonly string _jsonFilePath = "AlhindResponse.json";

        public async Task<List<ApiBound>> GetFlightsAsync()
        {
            if (!File.Exists(_jsonFilePath))
            {
                return new List<ApiBound>();
            }

            var jsonData = await File.ReadAllTextAsync(_jsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var flightResponse = JsonSerializer.Deserialize<ApiAlHindResponse>(jsonData, options);
            if (flightResponse?.Journy?.FlightOptions == null)
            {
                return new List<ApiBound>();
            }

            return flightResponse.Journy.FlightOptions
                .SelectMany(AlHindMapping.AlHindFlights)
                .ToList();
        }
    }
}
