using System;
using System.Threading.Tasks;
using App.Server.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace App.Server.Hubs
{
    public abstract class ChatHubBase : Hub
    {
        private readonly ChatOptions _options;
        
        protected string ClientNick => Context.User.Identity.Name;
        
        protected ChatHubBase(IOptions<ChatOptions> options)
        {
            _options = options.Value;
        }
        
        protected string ConnectionId => Context.ConnectionId;

        public override async Task OnConnectedAsync()
        {
            foreach (var chat in _options.Chats)
                await Groups.AddToGroupAsync(ConnectionId, chat);

            await Groups.AddToGroupAsync(ConnectionId, ClientNick);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var chat in _options.Chats)
                await Groups.RemoveFromGroupAsync(ConnectionId, chat);
            
            await Groups.RemoveFromGroupAsync(ConnectionId, ClientNick);
        }
    }
}