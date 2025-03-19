using FlightSystem.Models;

namespace FlightSystem.Services
{
    public interface IFlightService
    {
        Task<List<FlightDetails>> GetFlightsAsync();
    }
}
