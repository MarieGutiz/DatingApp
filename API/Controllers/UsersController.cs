using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiControler
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository  userRepository, IMapper mapper)
        {
            this._mapper = mapper;
            this.userRepository = userRepository;
        }

        //To get all users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers(){
           
           var users= await userRepository.GetMembersAsync();
        //   var usersToReturn= _mapper.Map<IEnumerable<MemberDto>>(users);
           return Ok(users);
          
        }

        //To get an specific user e.g api/user/3
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username){
           
         return await userRepository.GetMemberAsync(username);

         // return  _mapper.Map<MemberDto>(user);
           
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto){

            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user= await userRepository.GetUserByUsernameAsync(username);

            _mapper.Map(memberUpdateDto,user);

            userRepository.Update(user);

            if(await userRepository.SaveAllAsync()) return NoContent();

           return BadRequest("Failed to update user profile");
        }
    }
}