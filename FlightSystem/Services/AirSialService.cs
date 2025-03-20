using FlightSystem.Models;
using System.Text.Json;

namespace FlightSystem.Services
{
    public class AirSialService : IAirSialFlightServices
    {
        private readonly string _airSialPath = "AirsialResponse.json";
        public async Task<List<FlightBounding>> GetFlightsAsync()
        {
            var journeys = new List<FlightBounding>();

            if (File.Exists(_airSialPath))
            {
                var jsonData = await File.ReadAllTextAsync(_airSialPath);
                using JsonDocument doc = JsonDocument.Parse(jsonData);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Response", out JsonElement response) ||
                    !response.TryGetProperty("Data", out JsonElement data))
                {
                    return journeys;
                }

                var outboundFlights = new List<Bound>();
                var inboundFlights = new List<Bound>();

                if (data.TryGetProperty("outbound", out JsonElement outbound))
                {
                    foreach (var flight in outbound.EnumerateArray())
                    {
                        outboundFlights.Add(ParseFlight(flight));
                    }
                }

                if (data.TryGetProperty("inbound", out JsonElement inbound))
                {
                    foreach (var flight in inbound.EnumerateArray())
                    {
                        inboundFlights.Add(ParseFlight(flight));
                    }
                }

                foreach (var outboundFlight in outboundFlights)
                {
                    var matchingInbound = inboundFlights.FirstOrDefault(f =>
                        f.Origin == outboundFlight.Destination &&
                        f.Destination == outboundFlight.Origin &&
                        DateTime.Parse(f.DepartureDate) >= DateTime.Parse(outboundFlight.DepartureDate));

                    if (matchingInbound != null)
                    {
                        inboundFlights.Remove(matchingInbound);
                    }

                    journeys.Add(new FlightBounding
                    {
                        AirlineName = "AirSial",
                        OutboundJourney = outboundFlight,
                        InboundJourney = matchingInbound
                    });
                }

                // Add remaining inbound flights without matching outbound flights
                foreach (var inboundFlight in inboundFlights)
                {
                    journeys.Add(new FlightBounding
                    {
                        AirlineName = "AirSial",
                        OutboundJourney = null,
                        InboundJourney = inboundFlight
                    });
                }
            }
            return journeys;
        }

        private Bound ParseFlight(JsonElement flight)
        {
            var baggageFareDetails = new List<BaggageFare>();

            if (flight.TryGetProperty("BAGGAGE_FARE", out JsonElement baggageDetails))
            {
                foreach (var baggage in baggageDetails.EnumerateArray())
                {
                    var price = new Dictionary<string, PriceBreakdown>();

                    if (baggage.TryGetProperty("FARE_PAX_WISE", out JsonElement farePaxWiseElement))
                    {
                        foreach (var farePrice in farePaxWiseElement.EnumerateObject())
                        {
                            price[farePrice.Name] = new PriceBreakdown
                            {
                                BasePrice = farePrice.Value.TryGetProperty("BASIC_FARE", out JsonElement baseFare) ? GetDecimalValue(baseFare) : 0,
                                Charges = farePrice.Value.TryGetProperty("SURCHARGE", out JsonElement surcharge) ? GetDecimalValue(surcharge) : 0,
                                Fees = farePrice.Value.TryGetProperty("FEES", out JsonElement fees) ? GetDecimalValue(fees) : 0,
                                Taxs = farePrice.Value.TryGetProperty("TAX", out JsonElement tax) ? GetDecimalValue(tax) : 0,
                                TotalPrice = farePrice.Value.TryGetProperty("TOTAL", out JsonElement totalFare) ? GetDecimalValue(totalFare) : 0
                            };
                        }
                    }

                    baggageFareDetails.Add(new BaggageFare
                    {
                        ClassType = baggage.TryGetProperty("sub_class_desc", out JsonElement subClassDesc) ? subClassDesc.GetString() : string.Empty,
                        Bags = baggage.TryGetProperty("no_of_bags", out JsonElement noOfBags) ? noOfBags.GetString() : string.Empty,
                        Amount = baggage.TryGetProperty("amount", out JsonElement amount) ? amount.GetInt32() : 0,
                        ActualAmount = baggage.TryGetProperty("actual_amount", out JsonElement actualAmount) ? actualAmount.GetInt32() : 0,
                        Weight = baggage.TryGetProperty("weight", out JsonElement weight) ? weight.GetString() : string.Empty,
                        FarePaxWise = price
                    });
                }
            }

            string fullFlightCode = flight.TryGetProperty("FLIGHT_NO", out JsonElement flightNo) ? flightNo.GetString() : string.Empty;
            string flightNumber = new string(fullFlightCode.Where(char.IsDigit).ToArray());
            string airlineCode = new string(fullFlightCode.Where(char.IsLetter).ToArray());

            return new Bound
            {
                FullFlightCode = fullFlightCode,
                FlightNo = flightNumber,
                AirlineCode = airlineCode,
                DepartureDate = flight.TryGetProperty("DEPARTURE_DATE", out JsonElement departureDate) ? departureDate.GetString() : string.Empty,
                DepartureTime = flight.TryGetProperty("DEPARTURE_TIME", out JsonElement departureTime) ? departureTime.GetString() : string.Empty,
                ArrivalDate = flight.TryGetProperty("DEPARTURE_DATE", out JsonElement arrivalDate) ? arrivalDate.GetString() : string.Empty,
                ArrivalTime = flight.TryGetProperty("ARRIVAL_TIME", out JsonElement arrivalTime) ? arrivalTime.GetString() : string.Empty,
                Origin = flight.TryGetProperty("ORGN", out JsonElement origin) ? origin.GetString() : string.Empty,
                Destination = flight.TryGetProperty("DEST", out JsonElement destination) ? destination.GetString() : string.Empty,
                Duration = flight.TryGetProperty("DURATION", out JsonElement duration) ? duration.GetString() : string.Empty,
                AvailableSeats = flight.TryGetProperty("SEATS_AVAILABLE", out JsonElement availableSeats) ? availableSeats.GetInt32() : 0,
                CabinType = flight.TryGetProperty("CABIN", out JsonElement cabinType) ? cabinType.GetString() : string.Empty,
                ClassCode = flight.TryGetProperty("CLASS_CODE", out JsonElement classCode) ? classCode.GetString() : string.Empty,
                Currency = flight.TryGetProperty("CURRENCY", out JsonElement currency) ? currency.GetString() : string.Empty,
                BaggageFareDetails = baggageFareDetails,

            };
        }

        private decimal GetDecimalValue(JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Number ? element.GetDecimal() : decimal.Parse(element.GetString());
        }
    }
}
