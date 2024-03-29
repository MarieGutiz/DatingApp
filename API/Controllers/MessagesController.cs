using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiControler
    {
        private readonly IUnitOfWork _unitOfWork;
     
        private readonly IMapper _mapper;
        public MessagesController( IMapper mapper, IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;

        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto){
          
          var username = User.GetUsername();

          if(username == createMessageDto.RecipientUsername.ToLower())
           return BadRequest("You cannot send message to yourself");

           var sender = await _unitOfWork.userRepository.GetUserByUsernameAsync(username);
           var recipient = await _unitOfWork.userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
          
            if(recipient == null) return NotFound();

          var message = new Message{
             Sender= sender,
             Recipient = recipient,
             SenderUsername = sender.UserName,
             RecipientUsername = recipient.UserName,
             Content = createMessageDto.Content
          };
         
          _unitOfWork.messageRepository.AddMessage(message);

          if(await _unitOfWork.Completed()) return Ok(_mapper.Map<MessageDto>(message));

          return BadRequest("Failed to send message");

         
        }

         //create an end point for pagination
         [HttpGet]
         public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams){

             messageParams.Username = User.GetUsername();
             var message = await _unitOfWork.messageRepository.GetMessagesForUser(messageParams);

             Response.AddPaginationHeader(message.CurrentPage, message.PageSize, message.TotalCount, message.TotalPages);
            
            return message;
         }
          

        [HttpDelete("{id}")]

        public async Task<ActionResult> DeleteMessage(int id){

            var username = User.GetUsername();
            
            var message  = await _unitOfWork.messageRepository.GetMessage(id);

            if(message.Sender.UserName != username && message.Recipient.UserName != username)
            return Unauthorized();

            if(message.Sender.UserName == username) message.SenderDeleted = true;

            if(message.Recipient.UserName == username) message.RecipientDeleted = true;

            if(message.SenderDeleted && message.RecipientDeleted)
            _unitOfWork.messageRepository.DeleteMessage(message);

            if(await _unitOfWork.Completed()) return Ok();

            return BadRequest("Problem deleting the message");

        }

        //  [HttpGet("thread/{username}")]//We always got access to the username inside the controller

        // public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username){
           
        //    var currentUser = User.GetUsername();

        //    return  Ok(await _unitOfWork.messageRepository.GetMessageThread(currentUser, username));
        // }

    }
}