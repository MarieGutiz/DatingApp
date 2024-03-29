using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<PresenceHub> _presenceHub;
         private readonly PresenceTracker _tracker;
        public MessageHub(IMapper mapper,   IUnitOfWork unitOfWork,         
        IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
        {
            this._tracker = tracker;
            this._presenceHub = presenceHub;
            this._mapper = mapper;
            this._unitOfWork = unitOfWork;
        }

        public override async Task OnConnectedAsync(){
            
           //Get a group name
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId,groupName);
            var group =  await AddToGroup(groupName);//error
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messsages = await _unitOfWork.messageRepository
            .GetMessageThread(Context.User.GetUsername(), otherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messsages);
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group =  await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup",group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto){

             var username = Context.User.GetUsername();             

          if(username == createMessageDto.RecipientUsername.ToLower())
           throw new HubException("You cannot send message to yourself");

           var sender = await _unitOfWork.userRepository.GetUserByUsernameAsync(username);
           var recipient = await _unitOfWork.userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
          
            if(recipient == null) throw new HubException("Not found user");

          var message = new Message{
             Sender= sender,
             Recipient = recipient,
             SenderUsername = sender.UserName,
             RecipientUsername = recipient.UserName,
             Content = createMessageDto.Content
          };
         
         var groupName = GetGroupName(sender.UserName, recipient.UserName);
         var group = await _unitOfWork.messageRepository.GetMessageGroup(groupName);
        
        if(group.Connections.Any(x =>x.Username == recipient.UserName)){
          //They r connected to the same chat
          message.DateRead = DateTime.UtcNow;
        }
        else{
            //No connected 
          var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
          
          if(connections != null){
              Console.Write("Not in the hub");
              await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
              new {username = sender.UserName, knownAs = sender.KnownAs});
          }

        }

          _unitOfWork.messageRepository.AddMessage(message);

          if(await _unitOfWork.Completed()) 
          {
             await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
          }
         
        }

        private async Task<Group> AddToGroup(string groupName){

            var group = await _unitOfWork.messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId,Context.User.GetUsername());

            if(group == null){
                group = new Group(groupName);
                _unitOfWork.messageRepository.AddGroup(group);//error
            }
            group.Connections.Add(connection);

           if(await _unitOfWork.Completed()) return group;

           throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup(){

            var group  = await _unitOfWork.messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _unitOfWork.messageRepository.RemoveConnection(connection);

           if(await _unitOfWork.Completed()) return group;

           throw new HubException("Failed to remove from group");

        }
        private string GetGroupName(string caller, string other){//Return an alphabetical order

            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}":$"{other}-{caller}";
        }
    }
}