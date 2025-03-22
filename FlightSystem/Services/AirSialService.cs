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

                decimal BaseFare = 0;
                decimal Charges = 0;
                decimal Fees = 0;
                decimal Taxes = 0;
                decimal TotalPrice = 0;

                foreach (var baggage in baggageDetails.EnumerateArray())
                {
                    var ticketPrices = new List<PriceBreakdown>();

                    if (baggage.TryGetProperty("FARE_PAX_WISE", out JsonElement farePaxWiseElement))
                    {
                        foreach (var farePrice in farePaxWiseElement.EnumerateObject())
                        {

                            decimal baseFare = farePrice.Value.TryGetProperty("BASIC_FARE", out JsonElement baseFareElement) ? GetDecimalValue(baseFareElement) : 0;
                            decimal surcharge = farePrice.Value.TryGetProperty("SURCHARGE", out JsonElement surchargeElement) ? GetDecimalValue(surchargeElement) : 0;
                            decimal fees = farePrice.Value.TryGetProperty("FEES", out JsonElement feesElement) ? GetDecimalValue(feesElement) : 0;
                            decimal tax = farePrice.Value.TryGetProperty("TAX", out JsonElement taxElement) ? GetDecimalValue(taxElement) : 0;
                            decimal totalFare = farePrice.Value.TryGetProperty("TOTAL", out JsonElement totalFareElement) ? GetDecimalValue(totalFareElement) : 0;

                            ticketPrices.Add(new PriceBreakdown
                            {
                                BasePrice = baseFare,
                                Charges = surcharge,
                                Fees = fees,
                                Taxs = tax,
                                TotalPrice = totalFare
                            });

                            BaseFare += baseFare;
                            Charges += surcharge;
                            Fees += fees;
                            Taxes += tax;
                            TotalPrice += totalFare;
                        }
                    }

                    var totalFlightFare = new TotalPriceBreakdown
                    {
                        BasePrice = BaseFare,
                        Charges = Charges,
                        Fees = Fees,
                        Taxs = Taxes,
                        TotalPrice = TotalPrice
                    };


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
                        ArrivalDate = flight.TryGetProperty("DEPARTURE_DATE", out JsonElement arrivalDate) ? arrivalDate.GetString() : string.Empty,
                        ArrivalTime = flight.TryGetProperty("ARRIVAL_TIME", out JsonElement arrivalTime) ? arrivalTime.GetString() : string.Empty,
                        Origin = flight.TryGetProperty("ORGN", out JsonElement origin) ? origin.GetString() : string.Empty,
                        Destination = flight.TryGetProperty("DEST", out JsonElement destination) ? destination.GetString() : string.Empty,
                        Duration = flight.TryGetProperty("DURATION", out JsonElement duration) ? duration.GetString() : string.Empty,
                        AvailableSeats = flight.TryGetProperty("SEATS_AVAILABLE", out JsonElement availableSeats) ? availableSeats.GetInt32() : 0,
                        CabinType = flight.TryGetProperty("CABIN", out JsonElement cabinType) ? cabinType.GetString() : string.Empty,
                        ClassCode = baggageFare.ClassType, // Assign Class Type
                        Currency = flight.TryGetProperty("CURRENCY", out JsonElement currency) ? currency.GetString() : string.Empty,
                        TotalFlightFare = new List<TotalPriceBreakdown> { totalFlightFare },
                        BaggageFareDetails = new List<BaggageFare> { baggageFare }
                    });
                }
            }
            return flightList;
        }

        private decimal GetDecimalValue(JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Number ? element.GetDecimal() :
                   decimal.TryParse(element.GetString(), out decimal value) ? value : 0; 
        }
    }
}
