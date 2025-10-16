using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace myapp.Hubs
{
    public class ChatHub : Hub
    {
        // This method can be called by a client to join a specific chat session group.
        public async Task JoinChatSession(string sessionId)
        {
            // The Group name is based on the session ID to isolate communication.
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
        }

        // This method can be called by a client to leave a chat session group.
        public async Task LeaveChatSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session-{sessionId}");
        }

        // Server-side methods can call this to send messages to clients in a group.
        // For example: await _hubContext.Clients.Group($"session-{sessionId}").SendAsync("ReceiveMessage", user, message);
    }
}
