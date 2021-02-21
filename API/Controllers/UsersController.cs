using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiControler
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository  userRepository, IMapper mapper, IPhotoService photoService)
        {
            this._photoService = photoService;
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
        [HttpGet("{username}", Name="GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username){
           
         return await userRepository.GetMemberAsync(username);

         // return  _mapper.Map<MemberDto>(user);
           
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto){

            var user= await userRepository.GetUserByUsernameAsync(User.GetUsername());

            _mapper.Map(memberUpdateDto,user);

            userRepository.Update(user);

            if(await userRepository.SaveAllAsync()) return NoContent();

           return BadRequest("Failed to update user profile");
        }

        [HttpPost("add-photo")]

        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file){

           var user= await userRepository.GetUserByUsernameAsync(User.GetUsername());
           
           var result= await _photoService.AddPhotoAsync(file);

           if(result.Error != null)return BadRequest(result.Error.Message);

           var photo = new Photo{
               Url = result.SecureUrl.AbsoluteUri,
               PublicId = result.PublicId
           };
           if(user.Photos.Count == 0){
               photo.IsMain=true;
           }

            user.Photos.Add(photo);

            if(await userRepository.SaveAllAsync()){

               //return _mapper.Map<PhotoDto>(photo);//Return the photo
               return CreatedAtRoute("GetUser",new {username = user.UserName},_mapper.Map<PhotoDto>(photo));
            }
             

              return BadRequest("Problem adding photo");

        }

        [HttpPut("set-main-photo/{photoId}")]

        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo= user.Photos.FirstOrDefault(x=>x.Id == photoId);

            if (photo.IsMain) return BadRequest("This is already your main photo");
             
            var currentMain = user.Photos.FirstOrDefault(x=>x.IsMain);

            if(currentMain != null) currentMain.IsMain=false;//turn off the old main
            photo.IsMain=true; //set the new main

            if(await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
 
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId){

            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo= user.Photos.FirstOrDefault(x=>x.Id == photoId);

            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("You cannot delete your main photo");

            if(photo.PublicId != null){

                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if(await userRepository.SaveAllAsync()) return Ok();

            return  BadRequest("Failed to delete photo");
        }

    }
}