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

        public async Task<ApiAirSialResponse> GetFlightsAsync()
        {
            var response = new ApiAirSialResponse { Response = new List<FlightBounding>(), Success = false };

            if (!File.Exists(_airSialPath))
            {
                return response; // Return an empty response if the file does not exist
            }

            await using var stream = File.OpenRead(_airSialPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var airSialResponse = await JsonSerializer.DeserializeAsync<AirSialResponse>(stream, options);

            if (airSialResponse?.Response?.Data == null)
            {
                return response;
            }

            // Parse outbound and inbound flights
            var outboundFlights = airSialResponse.Response.Data.Outbound?.SelectMany(ParseFlight) ?? new List<ApiBound>();
            var inboundFlights = airSialResponse.Response.Data.Inbound?.SelectMany(ParseFlight) ?? new List<ApiBound>();

            var flights = outboundFlights.Select(outboundFlight => new FlightBounding
            {
                AirlineName = "AirSial",
                OutboundJourney = outboundFlight,
                InboundJourney = inboundFlights.FirstOrDefault(f =>
                    f.Origin == outboundFlight.Destination &&
                    f.Destination == outboundFlight.Origin &&
                    DateTime.TryParse(f.DepartureDate, out DateTime inboundDate) &&
                    DateTime.TryParse(outboundFlight.DepartureDate, out DateTime outboundDate) &&
                    inboundDate >= outboundDate)
            }).ToList();

            if (flights.Any())
            {
                response.Response = flights;
                response.Success = true;
            }

            return response;
        }

        private List<ApiBound> ParseFlight(AirSialFlight flight)
        {
            if (flight == null || flight.BaggageFare == null)
                return new List<ApiBound>();

            return flight.BaggageFare.Select(baggage => new ApiBound
            {
                FullFlightCode = flight.FlightNo,
                FlightNo = new string(flight.FlightNo.Where(char.IsDigit).ToArray()),
                AirlineCode = new string(flight.FlightNo.Where(char.IsLetter).ToArray()),
                DepartureDate = flight.DepartureDate,
                DepartureTime = flight.DepartureTime,
                ArrivalDate = flight.DepartureDate,
                ArrivalTime = flight.ArrivalTime,
                Origin = flight.Origin,
                Destination = flight.Destination,
                Duration = flight.Duration,
                AvailableSeats = flight.AvailableSeats,
                Currency = flight.Currency,
                CabinType = flight.Cabin,
                TotalFlightFare = new List<ApiTotalPriceBreakdown>
                {
                    new ApiTotalPriceBreakdown
                    {
                        BasePrice = baggage.FarePaxWise?.Values.Sum(p => p.GetDecimalValue(p.BasePrice)) ?? 0,
                        Charges = baggage.FarePaxWise?.Values.Sum(p => p.GetDecimalValue(p.Charges)) ?? 0,
                        Fees = baggage.FarePaxWise?.Values.Sum(p => p.GetDecimalValue(p.Fees)) ?? 0,
                        Taxs = baggage.FarePaxWise?.Values.Sum(p => p.GetDecimalValue(p.Taxs)) ?? 0,
                        TotalPrice = baggage.FarePaxWise?.Values.Sum(p => p.GetDecimalValue(p.TotalPrice)) ?? 0
                    }
                },
                BaggageFareDetails = new List<ApiBaggageFare>
                {
                    new ApiBaggageFare
                    {
                        ClassId = baggage.ClassId,
                        ClassType = baggage.ClassType,
                        Bags = baggage.Bags,
                        Amount = baggage.Amount,
                        ActualAmount = baggage.ActualAmount,
                        Weight = baggage.Weight,
                        FarePaxWise = baggage.FarePaxWise?.ToDictionary(
                            kvp => kvp.Key,
                            kvp => new ApiPriceBreakdown
                            {
                                BasePrice = kvp.Value.GetDecimalValue(kvp.Value.BasePrice),
                                Charges = kvp.Value.GetDecimalValue(kvp.Value.Charges),
                                Fees = kvp.Value.GetDecimalValue(kvp.Value.Fees),
                                Taxs = kvp.Value.GetDecimalValue(kvp.Value.Taxs),
                                TotalPrice = kvp.Value.GetDecimalValue(kvp.Value.TotalPrice)
                            }
                        ) ?? new Dictionary<string, ApiPriceBreakdown>()
                    }
                }
            }).ToList();
        }
    }

    public class ApiAirSialResponse
    {
        public required List<FlightBounding> Response { get; set; }
        public bool Success { get; set; }
    }

    public class FlightBounding
    {
        public required string AirlineName { get; set; }
        public ApiBound? OutboundJourney { get; set; }
        public ApiBound? InboundJourney { get; set; }
    }

    public class AirSialResponse
    {
        public required AirSialData Response { get; set; }
    }

    public class AirSialData
    {
        public required FlightData Data { get; set; }
    }

    public class FlightData
    {
        public required List<AirSialFlight> Outbound { get; set; }
        public required List<AirSialFlight> Inbound { get; set; }
    }

    public class AirSialFlight
    {
        [JsonPropertyName("FLIGHT_NO")]
        public required string FlightNo { get; set; }

        [JsonPropertyName("DEPARTURE_DATE")]
        public required string DepartureDate { get; set; }

        [JsonPropertyName("DEPARTURE_TIME")]
        public required string DepartureTime { get; set; }

        [JsonPropertyName("ARRIVAL_TIME")]
        public required string ArrivalTime { get; set; }

        [JsonPropertyName("ORGN")]
        public required string Origin { get; set; }

        [JsonPropertyName("DEST")]
        public required string Destination { get; set; }

        [JsonPropertyName("DURATION")]
        public required string Duration { get; set; }

        [JsonPropertyName("SEATS_AVAILABLE")]
        public int AvailableSeats { get; set; }

        [JsonPropertyName("CABIN")]
        public string? Cabin { get; set; }

        [JsonPropertyName("CURRENCY")]
        public required string Currency { get; set; }

        [JsonPropertyName("BAGGAGE_FARE")]
        public required List<AirSialFare> BaggageFare { get; set; }
    }

    public class AirSialFare
    {
        [JsonPropertyName("sub_class_id")]
        public required string ClassId { get; set; }

        [JsonPropertyName("sub_class_desc")]
        public required string ClassType { get; set; }

        [JsonPropertyName("no_of_bags")]
        public required string Bags { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("actual_amount")]
        public int ActualAmount { get; set; }

        [JsonPropertyName("weight")]
        public required string Weight { get; set; }

        [JsonPropertyName("FARE_PAX_WISE")]
        public required Dictionary<string, AirSialPaxWise> FarePaxWise { get; set; }
    }

    public class AirSialPaxWise
    {
        [JsonPropertyName("BASIC_FARE")]
        public required object BasePrice { get; set; }

        [JsonPropertyName("SURCHARGE")]
        public required object Charges { get; set; }

        [JsonPropertyName("FEES")]
        public required object Fees { get; set; }

        [JsonPropertyName("TAX")]
        public required object Taxs { get; set; }

        [JsonPropertyName("TOTAL")]
        public required object TotalPrice { get; set; }

        public decimal GetDecimalValue(object value)
        {
            if (value == null)
            {
                return 0;
            }
            return decimal.TryParse(value.ToString(), out decimal result) ? result : 0;
        }
    }
}