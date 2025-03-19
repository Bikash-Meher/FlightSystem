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
        public Dictionary<string, FareBreakdown> FareDetails { get; set; }
    }

    public class FareBreakdown
    {
        public decimal BaseFare { get; set; }
        public decimal Surcharge { get; set; }
        public decimal Fees { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalFare { get; set; }
       
    }


}
