using FlightSystem.Models;

namespace FlightSystem.Services
{
    public interface IAirSialFlightServices
    {
        Task<ApiAirSialResponse> GetFlightsAsync();
    }
}
