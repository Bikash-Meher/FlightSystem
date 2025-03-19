using FlightSystem.Models;

namespace FlightSystem.Services
{
    public interface IAirSialFlightServices
    {
        Task<List<FlightBounding>> GetFlightsAsync();
    }
}
