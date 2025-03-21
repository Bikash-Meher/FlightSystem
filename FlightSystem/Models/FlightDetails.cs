using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace FlightSystem.Models
{

    public class FlightDetails
    {
        public string AirlineName { get; set; }
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
        public string FareName { get; set; }
        public List<TotalFareBreakdown> TotalFareDetails { get; set; }
        public Dictionary<string, FareBreakdown> FareDetails { get; set; }
    }

    public class TotalFareBreakdown
    {
        public decimal BaseFare { get; set; }
        public decimal TotalTaxes { get; set; }
        public decimal TotalFare { get; set; }
    }

    public class FareBreakdown
    {
        public decimal BaseFare { get; set; }
        public decimal Taxs { get; set; }
        public decimal TotalFare { get; set; }

    }

    public class PassengerFare
    {
        public string Type { get; set; }
        public FareBreakdown Fare { get; set; }

    }
}