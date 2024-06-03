using Api.Dtos;
using Api.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Api.Hubs
{
    public class ChatHub(ChatService chatService) :Hub
    {
        private ChatService _chatService = chatService;

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Come2Chat");
            await Clients.Caller.SendAsync("UserConnected");
        }
        public override async Task OnDisconnectedAsync(Exception exceptopn)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Come2Chat"); 
            var user =_chatService.GetUserByConnectionId(Context.ConnectionId);
            _chatService.RemoveUserFromList(user);
            await DisplayOnlineUsers();
            await base.OnDisconnectedAsync(exceptopn);
        }
        public async Task AddUserConnectionId(string name)
        {
            _chatService.AddUserConnectionId(name,Context.ConnectionId);
            await DisplayOnlineUsers();
        }
        
        public async Task ReceiveMessage(MessageDto message)
        {
            await Clients.Group("Come2Chat").SendAsync("newMessage",message);
        }

        // Create a private chat and notify the end user 
        public async Task CreatePrivateChat(MessageDto message)
        {
            string PrivateGroupName = GetPrivateGroupName(message.From, message.To);
            await Groups.AddToGroupAsync(Context.ConnectionId,PrivateGroupName);
            var toConnectionId = _chatService.GetConnectionIdByUser(message.To);
            await Groups.AddToGroupAsync(toConnectionId, PrivateGroupName);
            await Clients.Client(toConnectionId).SendAsync("OpenPrivateChat", message);
        }
        // receive a private message
        public async Task ReceivePrivateMessage(MessageDto message)
        {
            string PrivateGroupName = GetPrivateGroupName(message.From, message.To);
            await Clients.Group(PrivateGroupName).SendAsync("NewPrivateMessage", message);
        }

        // remove a private chat  and remove the connectors
        public async Task RemovePrivateChat(string from,string to)
        {
            string PrivateGroupName = GetPrivateGroupName(from, to);
            await Clients.Group(PrivateGroupName).SendAsync("ClosePrivateChat");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId,PrivateGroupName);

            var toConnectionId = _chatService.GetConnectionIdByUser(to);
            await Groups.RemoveFromGroupAsync(toConnectionId,PrivateGroupName);
        }
        private async Task DisplayOnlineUsers()
        {
            var OnlineUsers = _chatService.GetOnlineUsers();
            // Send The Online Users To The Clients
            await Clients.Groups("Come2Chat").SendAsync("OnlineUsers", OnlineUsers);
        }
        private string GetPrivateGroupName(string from,string to)
        {
            var stringCompare = string.CompareOrdinal(from, to) < 0;
            return stringCompare ? $"{from}-{to}" : $"{to}-{from}";
        }
    }
}
