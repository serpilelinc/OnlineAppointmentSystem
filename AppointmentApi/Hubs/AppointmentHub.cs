using Microsoft.AspNetCore.SignalR;

namespace AppointmentApi.Hubs
{
    public class AppointmentHub : Hub
    {
        // Clients can connect to this hub to receive real-time notifications
        // The server will push messages to connected clients using IHubContext
    }
}
