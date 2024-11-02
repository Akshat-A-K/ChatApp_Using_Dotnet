using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;


namespace ChatApp.Hubs
{
    public class ChatHub:Hub
    {
		public async Task SendMessage(string senderId, string receiverId, string messageContent)
		{
			await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, messageContent);
		}
	}
}