using System.Text.Json.Serialization;

namespace FlightSystem.Models
{
    public class FlightBounding
    {
        [JsonPropertyName("Airline")]
        public string AirlineName { get; set; }

        [JsonPropertyName("Outgoing Flight")]
        public Bound? OutboundJourney { get; set; }

        [JsonPropertyName("Incoming Flight")]
        public Bound? InboundJourney { get; set; }
    }

    public class Bound
    {
        [JsonPropertyName("Flight Code")]
        public string FullFlightCode { get; set; }
        
        [JsonPropertyName("Flight No")]
        public string FlightNo { get; set; }
        
        [JsonPropertyName("Airline Code")]
        public string AirlineCode { get; set; }
        
        [JsonPropertyName("Departure Date")]
        public string DepartureDate { get; set; }
        
        [JsonPropertyName("Departure Time")]
        public string DepartureTime { get; set; }
        
        [JsonPropertyName("Arrival Date")]
        public string ArrivalDate { get; set; }
        
        [JsonPropertyName("Arival Time")]
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
        
        [JsonPropertyName("Departure Terminal")]
        public string DepartureTerminal { get; set; }
        
        [JsonPropertyName("Arrival Terminal")]
        public string ArrivalTerminal { get; set; }
        
        [JsonPropertyName("Currency Type")]
        public string Currency { get; set; }
        
        [JsonPropertyName("Luggage Charges")]
        public List<BaggageFare> BaggageFareDetails { get; set; } 


    }

    public class BaggageFare
    {
        [JsonPropertyName("Class Type")]
        public string ClassType { get; set; }
        
        [JsonPropertyName("No of Bags")]
        public string Bags { get; set; }
        
        [JsonPropertyName("Amount")]
        public int Amount { get; set; }
        
        [JsonPropertyName("Final Price")]
        public int ActualAmount { get; set; }
        
        [JsonPropertyName("Weight")]
        public string Weight { get; set; }
        
        [JsonPropertyName("Ticket Prices")]
        public Dictionary<string, PriceBreakdown> FarePaxWise { get; set; }

    }

    public class PriceBreakdown
    {
        [JsonPropertyName("Base Amount")]
        public decimal BasePrice { get; set; }
        
        [JsonPropertyName("Extra Charges")]
        public decimal Charges { get; set; }
        
        [JsonPropertyName("Fees")]
        public decimal Fees { get; set; }
        
        [JsonPropertyName("Taxes")]
        public decimal Taxs { get; set; }
        
        [JsonPropertyName("Total Amount")]
        public decimal TotalPrice { get; set; }
        
    }
}
