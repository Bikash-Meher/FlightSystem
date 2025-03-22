using FlightSystem.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace FlightSystem.Services
{
    public class AirSialService : IAirSialFlightServices
    {
        private readonly string _airSialPath = "AirsialResponse.json";
        private Bound returnDetails;

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
                        outboundFlights.AddRange(ParseFlight(flight)); // ✅ Fix: Handle multiple flights per input
                    }
                }

                if (data.TryGetProperty("inbound", out JsonElement inbound))
                {
                    foreach (var flight in inbound.EnumerateArray())
                    {
                        inboundFlights.AddRange(ParseFlight(flight)); // ✅ Fix: Handle multiple flights per input
                    }
                }

                foreach (var outboundFlight in outboundFlights)
                {
                    var matchingInbound = inboundFlights.FirstOrDefault(f =>
                        f.Origin == outboundFlight.Destination &&
                        f.Destination == outboundFlight.Origin &&
                        DateTime.TryParse(f.DepartureDate, out DateTime outboundDate) &&
                        DateTime.TryParse(f.DepartureDate, out DateTime inboundDate) &&
                        inboundDate >= outboundDate);

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
            }
            return journeys;
        }

        private List<Bound> ParseFlight(JsonElement flight)
        {
            var baggageFareDetails = new List<BaggageFare>();
            var flightList = new List<Bound>();

            if (flight.TryGetProperty("BAGGAGE_FARE", out JsonElement baggageDetails))
            {
                string fullFlightCode = flight.TryGetProperty("FLIGHT_NO", out JsonElement flightNo) ? flightNo.GetString() : string.Empty;
                string flightNumber = new string(fullFlightCode.Where(char.IsDigit).ToArray());
                string airlineCode = new string(fullFlightCode.Where(char.IsLetter).ToArray());

                foreach (var baggage in baggageDetails.EnumerateArray())
                {
                    var ticketPrices = new List<PriceBreakdown>();

                    if (baggage.TryGetProperty("FARE_PAX_WISE", out JsonElement farePaxWiseElement))
                    {
                        foreach (var farePrice in farePaxWiseElement.EnumerateObject())
                        {
                            ticketPrices.Add(new PriceBreakdown
                            {
                                BasePrice = farePrice.Value.TryGetProperty("BASIC_FARE", out JsonElement baseFare) ? GetDecimalValue(baseFare) : 0,
                                Charges = farePrice.Value.TryGetProperty("SURCHARGE", out JsonElement surcharge) ? GetDecimalValue(surcharge) : 0,
                                Fees = farePrice.Value.TryGetProperty("FEES", out JsonElement fees) ? GetDecimalValue(fees) : 0,
                                Taxs = farePrice.Value.TryGetProperty("TAX", out JsonElement tax) ? GetDecimalValue(tax) : 0,
                                TotalPrice = farePrice.Value.TryGetProperty("TOTAL", out JsonElement totalFare) ? GetDecimalValue(totalFare) : 0
                            });
                        }
                    }

                    var baggageFare = new BaggageFare
                    {
                        ClassId = baggage.TryGetProperty("sub_class_id", out JsonElement subClassId) &&
                                  byte.TryParse(subClassId.GetString(), out byte result)
                            ? result
                            : (byte)0,
                        ClassType = baggage.TryGetProperty("sub_class_desc", out JsonElement subClassDesc) ? subClassDesc.GetString() : string.Empty,
                        Bags = baggage.TryGetProperty("no_of_bags", out JsonElement noOfBags) ? noOfBags.GetString() : string.Empty,
                        Amount = baggage.TryGetProperty("amount", out JsonElement amount) ? amount.GetInt32() : 0,
                        ActualAmount = baggage.TryGetProperty("actual_amount", out JsonElement actualAmount) ? actualAmount.GetInt32() : 0,
                        Weight = baggage.TryGetProperty("weight", out JsonElement weight) ? weight.GetString() : string.Empty,
                        FarePaxWise = ticketPrices
                    };

                    baggageFareDetails.Add(baggageFare);

                    flightList.Add(new Bound
                    {
                        FullFlightCode = fullFlightCode,
                        FlightNo = flightNumber,
                        AirlineCode = airlineCode,
                        DepartureDate = flight.TryGetProperty("DEPARTURE_DATE", out JsonElement departureDate) ? departureDate.GetString() : string.Empty,
                        DepartureTime = flight.TryGetProperty("DEPARTURE_TIME", out JsonElement departureTime) ? departureTime.GetString() : string.Empty,
                        ArrivalDate = flight.TryGetProperty("DEPARTURE_DATE", out JsonElement arrivalDate) ? arrivalDate.GetString() : string.Empty, // ✅ Fix: Correct arrival date
                        ArrivalTime = flight.TryGetProperty("ARRIVAL_TIME", out JsonElement arrivalTime) ? arrivalTime.GetString() : string.Empty,
                        Origin = flight.TryGetProperty("ORGN", out JsonElement origin) ? origin.GetString() : string.Empty,
                        Destination = flight.TryGetProperty("DEST", out JsonElement destination) ? destination.GetString() : string.Empty,
                        Duration = flight.TryGetProperty("DURATION", out JsonElement duration) ? duration.GetString() : string.Empty,
                        AvailableSeats = flight.TryGetProperty("SEATS_AVAILABLE", out JsonElement availableSeats) ? availableSeats.GetInt32() : 0,
                        CabinType = flight.TryGetProperty("CABIN", out JsonElement cabinType) ? cabinType.GetString() : string.Empty,
                        ClassCode = baggageFare.ClassType, // Assign Class Type
                        Currency = flight.TryGetProperty("CURRENCY", out JsonElement currency) ? currency.GetString() : string.Empty,
                        BaggageFareDetails = new List<BaggageFare> { baggageFare }
                    });
                }
            }
            return flightList;
        }

        private decimal GetDecimalValue(JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Number ? element.GetDecimal() :
                   decimal.TryParse(element.GetString(), out decimal value) ? value : 0; // ✅ Fix: Prevent parsing errors
        }
    }
}
