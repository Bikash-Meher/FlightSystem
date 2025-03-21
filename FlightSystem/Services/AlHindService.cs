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
        private readonly string _alhindPath = "AlhindResponse.json";

        public async Task<List<FlightDetails>> GetFlightsAsync()
        {
            var journeys = new List<FlightDetails>();

            if (File.Exists(_alhindPath))
            {
                var jsonData = await File.ReadAllTextAsync(_alhindPath);
                using JsonDocument doc = JsonDocument.Parse(jsonData);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Journy", out JsonElement journey) ||
                    !journey.TryGetProperty("FlightOptions", out JsonElement flightOptions))
                {
                    return journeys;
                }

                foreach (var flight in flightOptions.EnumerateArray())
                {
                    var flightLeg = flight.GetProperty("FlightLegs").EnumerateArray().First();
                    string flightNumber = flightLeg.GetProperty("FlightNo").GetString();
                    string airlineCode = flightLeg.GetProperty("AirlineCode").GetString();
                    string fullFlightCode = $"{airlineCode}{flightNumber}";
                    DateTime departureDateTime = DateTime.Parse(flightLeg.GetProperty("DepartureTime").GetString());
                    DateTime arrivalDateTime = DateTime.Parse(flightLeg.GetProperty("ArrivalTime").GetString());
                    TimeSpan duration = arrivalDateTime - departureDateTime;


                    var flightFareOptions = flight.GetProperty("FlightFares").EnumerateArray();

                    foreach (var fareOption in flightFareOptions)
                    {
                        string fareName = fareOption.GetProperty("FareName").GetString();
                        var fareDetails = new Dictionary<string, FareBreakdown>();

                        var fares = fareOption.GetProperty("Fares").EnumerateArray();

                        foreach (var fare in fares)
                        {
                            string ptc = fare.GetProperty("PTC").GetString();
                            decimal baseFare = fare.GetProperty("BaseFare").GetDecimal();
                            decimal tax = fare.GetProperty("Tax").GetDecimal();
                            decimal discount = fare.TryGetProperty("Discount", out JsonElement discountElement) ? discountElement.GetDecimal() : 0;
                            decimal totalFare = baseFare + tax - discount;

                            fareDetails[ptc] = new FareBreakdown
                            {
                                BaseFare = baseFare,
                                Taxs = tax,
                                TotalFare = totalFare
                            };
                        }


                        var aprxFareTotal = new List<TotalFareBreakdown>();

                        aprxFareTotal.Add(new TotalFareBreakdown
                        {
                            BaseFare = fareOption.GetProperty("AprxTotalBaseFare").GetDecimal(),
                            TotalTaxes = fareOption.GetProperty("AprxTotalTax").GetDecimal(),
                            TotalFare = fareOption.GetProperty("AprxTotalAmount").GetDecimal()
                        });


                        journeys.Add(new FlightDetails
                        {
                            AirlineName = "AlHind",
                            FullFlightCode = fullFlightCode,
                            FlightNo = flightNumber,
                            AirlineCode = airlineCode,
                            FareName = fareName,
                            DepartureDate = departureDateTime.ToString("yyyy-MM-dd"),
                            DepartureTime = departureDateTime.ToString("HH:mm"),
                            ArrivalDate = arrivalDateTime.ToString("yyyy-MM-dd"),
                            ArrivalTime = arrivalDateTime.ToString("HH:mm"),
                            Origin = flightLeg.GetProperty("Origin").GetString(),
                            Destination = flightLeg.GetProperty("Destination").GetString(),
                            Duration = string.Format("{0}h {1}m", duration.Hours, duration.Minutes),
                            AvailableSeats = flight.GetProperty("AvailableSeat").GetInt32(),
                            CabinType = "Unknown Cabin",
                            ClassCode = "",
                            DepartureTerminal = flightLeg.GetProperty("DepartureTerminal").GetString(),
                            ArrivalTerminal = flightLeg.GetProperty("ArrivalTerminal").GetString(),
                            Currency = "",
                            TotalFareDetails = aprxFareTotal,
                            FareDetails = fareDetails
                        });
                    }
                }
            }

            return journeys;
        }
    }
}
