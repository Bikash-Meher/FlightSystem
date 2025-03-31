
namespace FlightSystem.Models
{
    public class ApiBound
    {
        public required string FullFlightCode { get; set; }
        public required string FlightNo { get; set; }
        public required string AirlineCode { get; set; }
        public required string DepartureDate { get; set; }
        public required string DepartureTime { get; set; }
        public required string ArrivalDate { get; set; }
        public required string ArrivalTime { get; set; }
        public required string Origin { get; set; }
        public required string Destination { get; set; }
        public required string Duration { get; set; }
        public int AvailableSeats { get; set; }
        public string? ClassCode { get; set; }
        public string? DepartureTerminal { get; set; }
        public string? ArrivalTerminal { get; set; }
        public string? Currency { get; set; }
        public required List<ApiPriceBreakdown> TotalFlightFare { get; set; }
        public required List<ApiBaggageFare> BaggageFareDetails { get; set; }

    }

    public class ApiBaggageFare
    {
        public string? ClassId { get; set; }
        public string? ClassType { get; set; }
        public string? Bags { get; set; }
        public int? Amount { get; set; }
        public int? ActualAmount { get; set; }
        public string? Weight { get; set; }
        public required Dictionary<string, ApiPriceBreakdown> FarePaxWise { get; set; }


    }

    public class ApiPriceBreakdown
    {
        public decimal BasePrice { get; set; }
        public decimal Charges { get; set; }
        public decimal Fees { get; set; }
        public decimal Taxs { get; set; }
        public decimal TotalPrice { get; set; }

    }

    //public class ApiTotalPriceBreakdown
    //{
    //    public decimal BasePrice { get; set; }
    //    public decimal Charges { get; set; }
    //    public decimal Fees { get; set; }
    //    public decimal Taxs { get; set; }
    //    public decimal TotalPrice { get; set; }

    //}
}

