using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace FlightSystem.Models
{
    public class AlHindResponse
    {
        [JsonPropertyName("Journy")]
        public Journy Journy { get; set; }
    }

    public class Journy
    {
        [JsonPropertyName("FlightOptions")]
        public List<FlightOption> FlightOptions { get; set; }
    }

    public class FlightOption
    {
        public string Key { get; set; }
        public string TicketingCarrier { get; set; }
        public int AvailableSeat { get; set; }
        public List<FlightLeg> FlightLegs { get; set; }
        public List<FlightFare> FlightFares { get; set; }
    }

    public class FlightLeg
    {
        public string FlightNo { get; set; }
        public string AirlineCode { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalTime { get; set; }
        public string DepartureTerminal { get; set; }
        public string ArrivalTerminal { get; set; }
    }

    public class FlightFare
    {
        public string FareName { get; set; }
        public decimal AprxTotalBaseFare { get; set; }
        public decimal AprxTotalTax { get; set; }
        public decimal AprxTotalAmount { get; set; }
        public List<FareDetail> Fares { get; set; }
    }

    public class FareDetail
    {
        public string PTC { get; set; }
        public decimal BaseFare { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
    }
    public class AlhindPriceBreakdown
    {
        public decimal BaseFare { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
    }

    public class FlightOptionMapper
    {
        public static List<ApiBound> MapToApiBoundList(FlightOption flightOption)
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
                    Duration = string.Format($"{duration.Hours}h { duration.Minutes}m"),
                    AvailableSeats = flightOption.AvailableSeat,
                    DepartureTerminal = flightLeg.DepartureTerminal,
                    ArrivalTerminal = flightLeg.ArrivalTerminal,
                    Currency ="INR" ,
                    TotalFlightFare = new List<ApiTotalPriceBreakdown>
                    {
                        new ApiTotalPriceBreakdown
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
                    }
                };

                apiBounds.Add(apiBound);
            }

            return apiBounds;
        }
        private static List<ApiBaggageFare> MapBaggageFareDetails(FlightFare fareOption)
        {
            var baggageFare = new ApiBaggageFare
            {
                ClassType = fareOption.FareName,
                FarePaxWise = fareOption.Fares.ToDictionary(detail => detail.PTC, detail => new ApiPriceBreakdown
                {
                    BasePrice = detail.BaseFare,
                    Taxs = detail.Tax,
                    TotalPrice = detail.BaseFare + detail.Tax - detail.Discount
                })
            };
            return new List<ApiBaggageFare> { baggageFare };
        }
    }
}
