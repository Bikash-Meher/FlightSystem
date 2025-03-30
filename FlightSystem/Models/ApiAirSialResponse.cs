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
        public string AirlineName { get; set; }
        public ApiBound? OutboundJourney { get; set; }
        public ApiBound? InboundJourney { get; set; }
    }

    public class AirSialResponse
    {
        public AirSialData Response { get; set; }
    }

    public class AirSialData
    {
        public FlightData Data { get; set; }
    }

    public class FlightData
    {
        public List<AirSialFlight> Outbound { get; set; } = new();
        public List<AirSialFlight> Inbound { get; set; } = new();
    }

    public class AirSialFlight
    {
        [JsonPropertyName("FLIGHT_NO")]
        public string FlightNo { get; set; }

        [JsonPropertyName("DEPARTURE_DATE")]
        public string DepartureDate { get; set; }

        [JsonPropertyName("DEPARTURE_TIME")]
        public string DepartureTime { get; set; }

        [JsonPropertyName("ARRIVAL_TIME")]
        public string ArrivalTime { get; set; }

        [JsonPropertyName("ORGN")]
        public string Origin { get; set; }

        [JsonPropertyName("DEST")]
        public string Destination { get; set; }

        [JsonPropertyName("DURATION")]
        public string Duration { get; set; }

        [JsonPropertyName("SEATS_AVAILABLE")]
        public int AvailableSeats { get; set; }

        [JsonPropertyName("CABIN")]
        public string Cabin { get; set; }

        [JsonPropertyName("CURRENCY")]
        public string Currency { get; set; }

        [JsonPropertyName("BAGGAGE_FARE")]
        public List<AirSialFare> BaggageFare { get; set; } = new();
    }

    public class AirSialFare
    {
        [JsonPropertyName("sub_class_id")]
        public string ClassId { get; set; }

        [JsonPropertyName("sub_class_desc")]
        public string ClassType { get; set; }

        [JsonPropertyName("no_of_bags")]
        public string Bags { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("actual_amount")]
        public int ActualAmount { get; set; }

        [JsonPropertyName("weight")]
        public string Weight { get; set; }

        [JsonPropertyName("FARE_PAX_WISE")]
        public Dictionary<string, AirSialPaxWise> FarePaxWise { get; set; } = new();
    }

    public class AirSialPaxWise
    {
        [JsonPropertyName("BASIC_FARE")]
        public object BasePrice { get; set; }

        [JsonPropertyName("SURCHARGE")]
        public object Charges { get; set; }

        [JsonPropertyName("FEES")]
        public object Fees { get; set; }

        [JsonPropertyName("TAX")]
        public object Taxs { get; set; }

        [JsonPropertyName("TOTAL")]
        public object TotalPrice { get; set; }

        public decimal GetDecimalValue(object value)
        {
            return decimal.TryParse(value?.ToString(), out var result) ? result : 0;
        }
    }

    public static class AirSialMapper
    {
        public static ApiAirSialResponse MapToCommonResponse(AirSialResponse airSialResponse)
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
                Currency = flight.Currency,
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

            return parseFlight;
        }
    }
}
