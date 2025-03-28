namespace FlightSystem.Models
{
    public class ApiBound
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
        public List<ApiTotalPriceBreakdown> TotalFlightFare { get; set; }
        public List<ApiBaggageFare> BaggageFareDetails { get; set; }


    }

    public class ApiBaggageFare
    {
        public string ClassId { get; set; }
        public string ClassType { get; set; }
        public string Bags { get; set; }
        public int Amount { get; set; }
        public int ActualAmount { get; set; }
        public string Weight { get; set; }
        public Dictionary<string, ApiPriceBreakdown> FarePaxWise { get; set; }


    }

    public class ApiPriceBreakdown
    {
        public decimal BasePrice { get; set; }
        public decimal Charges { get; set; }
        public decimal Fees { get; set; }
        public decimal Taxs { get; set; }
        public decimal TotalPrice { get; set; }

    }

    public class ApiTotalPriceBreakdown
    {
        public decimal BasePrice { get; set; }
        public decimal Charges { get; set; }
        public decimal Fees { get; set; }
        public decimal Taxs { get; set; }
        public decimal TotalPrice { get; set; }

    }
}

