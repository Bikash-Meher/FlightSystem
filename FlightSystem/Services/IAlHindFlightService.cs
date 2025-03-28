using FlightSystem.Models;
using static FlightSystem.Services.AlHindService;

namespace FlightSystem.Services
{
    public interface IAlHindFlightService
    {
        Task<List<AlHindFlightBounding>> GetFlightsAsync();
    }
}
