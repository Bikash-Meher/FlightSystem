using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace FlightSystem.Models
{
    public class ApiAlHindResponse
    {
        [JsonPropertyName("Journy")]
        public required Journy Journy { get; set; }
    }

    public class Journy
    {
        [JsonPropertyName("FlightOptions")]
        public required List<FlightOption> FlightOptions { get; set; }
    }

    public class FlightOption
    {
        [JsonPropertyName("AvailableSeat")]
        public int AvailableSeat { get; set; }

        [JsonPropertyName("FlightLegs")]
        public required List<FlightLeg> FlightLegs { get; set; }

        [JsonPropertyName("FlightFares")]
        public required List<FlightFare> FlightFares { get; set; }
    }

    public class FlightLeg
    {
        [JsonPropertyName("FlightNo")]
        public required string FlightNo { get; set; }

        [JsonPropertyName("AirlineCode")]
        public required string AirlineCode { get; set; }

        [JsonPropertyName("Origin")]
        public required string Origin { get; set; }

        [JsonPropertyName("Destination")]
        public required string Destination { get; set; }

        [JsonPropertyName("DepartureTime")]
        public required string DepartureTime { get; set; }

        [JsonPropertyName("ArrivalTime")]
        public required string ArrivalTime { get; set; }

        [JsonPropertyName("DepartureTerminal")]
        public required string DepartureTerminal { get; set; }

        [JsonPropertyName("ArrivalTerminal")]
        public required string ArrivalTerminal { get; set; }
    }

    public class FlightFare
    {
        [JsonPropertyName("FareName")]
        public required string FareName { get; set; }

        [JsonPropertyName("AprxTotalBaseFare")]
        public decimal AprxTotalBaseFare { get; set; }

        [JsonPropertyName("AprxTotalTax")]
        public decimal AprxTotalTax { get; set; }

        [JsonPropertyName("AprxTotalAmount")]
        public decimal AprxTotalAmount { get; set; }

        [JsonPropertyName("Fares")]
        public required List<FareDetail> Fares { get; set; }
    }

    public class FareDetail
    {
        [JsonPropertyName("PTC")]
        public required string PTC { get; set; }

        [JsonPropertyName("BaseFare")]
        public decimal BaseFare { get; set; }

        [JsonPropertyName("Tax")]
        public decimal Tax { get; set; }

        [JsonPropertyName("Discount")]
        public decimal Discount { get; set; }
    }
   
    public class AlHindMapping
    {
        public static List<ApiBound> AlHindFlights(FlightOption flightOption)
        {
            var apiBounds = new List<ApiBound>();

            foreach (var fareOption in flightOption.FlightFares)
            {
                var flightLeg = flightOption.FlightLegs.FirstOrDefault();
                if (flightLeg == null) continue;

                DateTime departureDateTime = DateTime.Parse(flightLeg.DepartureTime);
                DateTime arrivalDateTime = DateTime.Parse(flightLeg.ArrivalTime);
                TimeSpan duration = arrivalDateTime - departureDateTime;

                var apiBound = new ApiBound
                {
                    AirlineCode = flightLeg.AirlineCode,
                    FlightNo = flightLeg.FlightNo,
                    FullFlightCode = flightLeg.AirlineCode + flightLeg.FlightNo,
                    DepartureDate = departureDateTime.ToString("yyyy-MM-dd"),
                    DepartureTime = departureDateTime.ToString("HH:mm"),
                    ArrivalDate = arrivalDateTime.ToString("yyyy-MM-dd"),
                    ArrivalTime = arrivalDateTime.ToString("HH:mm"),
                    Origin = flightLeg.Origin,
                    Destination = flightLeg.Destination,
                    Duration = string.Format($"{duration.Hours}h {duration.Minutes}m"),
                    AvailableSeats = flightOption.AvailableSeat,
                    DepartureTerminal = flightLeg.DepartureTerminal,
                    ArrivalTerminal = flightLeg.ArrivalTerminal,
                    Currency = "INR",
                    TotalFlightFare = new List<ApiPriceBreakdown>
                    {
                        new ApiPriceBreakdown
                        {
                            BasePrice = fareOption.AprxTotalBaseFare,
                            Taxs = fareOption.AprxTotalTax,
                            TotalPrice = fareOption.AprxTotalAmount
                        }
                    },
                    BaggageFareDetails = new List<ApiBaggageFare>
                    {
                        new ApiBaggageFare
                        {
                            ClassType = fareOption.FareName,
                            FarePaxWise = fareOption.Fares.ToDictionary(detail => detail.PTC, detail => new ApiPriceBreakdown
                            {
                                BasePrice = detail.BaseFare,
                                Taxs = detail.Tax,
                                TotalPrice = detail.BaseFare + detail.Tax - detail.Discount
                            })
                        }
                    },
                                    };

                apiBounds.Add(apiBound);
            }

            return apiBounds;
        }
       
    }
}
