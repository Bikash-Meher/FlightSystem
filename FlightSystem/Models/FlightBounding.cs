using System.Text.Json.Serialization;

namespace FlightSystem.Models
{
    public class FlightBounding
    {
        public string AirlineName { get; set; }
        public Bound? OutboundJourney { get; set; }
        public Bound? InboundJourney { get; set; }
    }

    public class Bound
    {
        public string FullFlightCode { get; set; }
        public string FlightNo { get; set; }
        public string AirlineCode { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Duration { get; set; }
        public int AvailableSeats { get; set; }
        public string CabinType { get; set; }
        public string ClassCode { get; set; }
        public string DepartureTerminal { get; set; }
        public string ArrivalTerminal { get; set; }
        public string Currency { get; set; }
        public List<TotalPriceBreakdown> TotalFlightFare { get; set; }
        public List<BaggageFare> BaggageFareDetails { get; set; }


    }

    public class BaggageFare
    {
        public byte? ClassId { get; set; }
        public string ClassType { get; set; }
        public string Bags { get; set; }
        public int Amount { get; set; }
        public int ActualAmount { get; set; }
        public string Weight { get; set; }
        public List<PriceBreakdown> FarePaxWise { get; set; }

    }

    public class PriceBreakdown
    {
        public decimal BasePrice { get; set; }
        public decimal Charges { get; set; }
        public decimal Fees { get; set; }
        public decimal Taxs { get; set; }
        public decimal TotalPrice { get; set; }

    }

    public class TotalPriceBreakdown
    {
        public decimal BasePrice { get; set; }
        public decimal Charges { get; set; }
        public decimal Fees { get; set; }
        public decimal Taxs { get; set; }
        public decimal TotalPrice { get; set; }

    }
}
