using System.Threading.Tasks;
using App.Server.Hubs.Messages;
using App.Server.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace App.Server.Hubs
{
    /// <summary>
    /// SignalR entry point
    /// </summary>
    [Authorize]
    public class ChatHub : ChatHubBase
    {
        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="chatOptions"></param>
        public ChatHub(IOptionsSnapshot<ChatOptions> chatOptions) : base(chatOptions)
        {
        }
        
        /// <summary>
        /// Post message to chat
        /// </summary>
        /// <returns></returns>
        [HubMethodName("post")]
        public async Task PostMessage(PostMessage message)
        {
            await Clients.Group(message.Group).SendAsync("message", new
            {
                Message = message.Message,
                Group = message.Group,
                Nick = ClientNick
            });
        }

        /// <summary>
        /// Join chat
        /// </summary>
        /// <returns></returns>
        [HubMethodName("join")]
        public async Task JoinChat(JoinChatMessage message)
        {
            await Clients.Group(message.Group).SendAsync("joined", new {message.Group, Nick = ClientNick});
            await Groups.AddToGroupAsync(ConnectionId, message.Group);
        }

        /// <summary>
        /// Leave chat
        /// </summary>
        /// <returns></returns>
        [HubMethodName("leave")]
        public async Task LeaveChat(LeaveChatMessage message)
        {
            await Clients.OthersInGroup(message.Group).SendAsync("lost", new {message.Group, Nick = ClientNick});
            await Clients.Group(ClientNick).SendAsync("mirror", new
            {
                Method = "unsubscribe",
                Payload = new UnsubscribeChatMessage
                {
                    Group = message.Group
                }
            });
        }

        /// <summary>
        /// Unsubscribe from group
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HubMethodName("unsubscribe")]
        public async Task Unsubscribe(UnsubscribeChatMessage message)
        {
            await Groups.RemoveFromGroupAsync(ConnectionId, message.Group);
        }
    }
}