using System.Text.Json.Serialization;

namespace FlightSystem.Models
{

    public class FlightDetails
    {
        [JsonPropertyName("Airline")]
        public string AirlineName { get; set; }
        
        [JsonPropertyName("Flight Code")]
        public string FullFlightCode { get; set; }
        
        [JsonPropertyName("Flight Number")]
        public string FlightNo { get; set; }
        
        [JsonPropertyName("Airline Code")]
        public string AirlineCode { get; set; }
        
        [JsonPropertyName("Departure Date")]
        public string DepartureDate { get; set; }
        
        [JsonPropertyName("Departure Time")]
        public string DepartureTime { get; set; }
        
        [JsonPropertyName("Arrival Date")]
        public string ArrivalDate { get; set; }
        
        [JsonPropertyName("Arrival Time")]
        public string ArrivalTime { get; set; }
        
        [JsonPropertyName("Flight Boarding")]
        public string Origin { get; set; }
        
        [JsonPropertyName("Flight Destination")]
        public string Destination { get; set; }
        
        [JsonPropertyName("Travel Duration")]
        public string Duration { get; set; }
        
        [JsonPropertyName("Seat Available")]
        public int AvailableSeats { get; set; }
        
        [JsonPropertyName("Cabin Type")]
        public string CabinType { get; set; }
        
        [JsonPropertyName("Class Code")]
        public string ClassCode { get; set; }
        
        [JsonPropertyName("Departore Terminal")]
        public string DepartureTerminal { get; set; }
        
        [JsonPropertyName("Arrival Terminal")]
        public string ArrivalTerminal { get; set; }
        
        [JsonPropertyName("Currency Type")]
        public string Currency { get; set; }
        
        [JsonPropertyName("Ticket Price Details")]
        public Dictionary<string, FareBreakdown> FareDetails { get; set; }
    }

    public class FareBreakdown
    {
        [JsonPropertyName("Base Price")]
        public decimal BasePrice { get; set; }
        
        [JsonPropertyName("Taxes")]
        public decimal Taxs { get; set; }
        
        [JsonPropertyName("Total Price")]
        public decimal TotalPrice { get; set; }
       
    }


}
