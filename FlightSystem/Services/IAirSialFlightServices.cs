using FlightSystem.Models;

namespace FlightSystem.Services
{
    public interface IAirSialFlightServices
    {
        Task<ApiResponse> GetFlightsAsync();
    }
}
