using FlightSystem.Models;

namespace FlightSystem.Services
{
    public interface IAlHindFlightService
    {
        Task<List<ApiBound>> GetFlightsAsync();
    }
}
