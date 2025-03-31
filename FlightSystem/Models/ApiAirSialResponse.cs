using System.Text.Json.Serialization;

namespace FlightSystem.Models
{
    public class ApiAirSialResponse
    {
        public List<FlightBounding> Response { get; set; } = new();
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
        public List<AirSialFlight> Outbound { get; set; } = new();
        public List<AirSialFlight> Inbound { get; set; } = new();
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

        [JsonPropertyName("CLASS_CODE")]
        public string? ClassCode { get; set; }

        [JsonPropertyName("CURRENCY")]
        public required string Currency { get; set; }

        [JsonPropertyName("BAGGAGE_FARE")]
        public List<AirSialFare> BaggageFare { get; set; } = new();
    }

    public class AirSialFare
    {
        [JsonPropertyName("sub_class_id")]
        public string? ClassId { get; set; }

        [JsonPropertyName("sub_class_desc")]
        public required string ClassType { get; set; }

        [JsonPropertyName("no_of_bags")]
        public string? Bags { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("actual_amount")]
        public int ActualAmount { get; set; }

        [JsonPropertyName("weight")]
        public string? Weight { get; set; }

        [JsonPropertyName("FARE_PAX_WISE")]
        public Dictionary<string, AirSialPaxWise> FarePaxWise { get; set; } = new();
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
            return decimal.TryParse(value?.ToString(), out var result) ? result : 0;
        }
    }

    public static class AirSialMapping
    {
        public static ApiAirSialResponse AirSialFlights(AirSialResponse airSialResponse)
        {
            var response = new ApiAirSialResponse { Response = new List<FlightBounding>(), Success = false };

            if (airSialResponse?.Response?.Data == null)
            {
                return response;
            }

            var outboundFlights = airSialResponse.Response.Data.Outbound?.SelectMany(ParseFlight) ?? new List<ApiBound>();
            var inboundFlights = airSialResponse.Response.Data.Inbound?.SelectMany(ParseFlight) ?? new List<ApiBound>();

            response.Response = outboundFlights.Select(outboundFlight => new FlightBounding
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

            response.Success = response.Response.Any();
            return response;
        }

        private static List<ApiBound> ParseFlight(AirSialFlight flight)
        {
            if (flight == null || flight.BaggageFare == null)
            {
                return new List<ApiBound>();
            }


            var parseFlight = flight.BaggageFare.Select(baggage => new ApiBound
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
                ClassCode = flight.ClassCode,
                Currency = flight.Currency,
                TotalFlightFare = new List<ApiPriceBreakdown>
                {
                    new ApiPriceBreakdown
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

            return parseFlight;
        }
    }
}
