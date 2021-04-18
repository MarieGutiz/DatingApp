using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize] //Authenticating
    public class LikesController : BaseApiControler
    {
        private readonly IUnitOfWork _unitOfWork;
       
        public LikesController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            

        }

        //The user they're gonna be liking
        [HttpPost("{Username}")]
        public async Task<ActionResult> AddLike(string username){
       
        var sourceUserId = User.GetUserId();
        var likedUser= await _unitOfWork.userRepository.GetUserByUsernameAsync(username);
        var sourceUser= await _unitOfWork.likesRepository.GetUserWithLike(sourceUserId);

        if(likedUser == null) return NotFound();

        //prevent user to like himself
        if(sourceUser.UserName == username ) return BadRequest("You cannot like yourself");

        var userLike = await _unitOfWork.likesRepository.getUserLike(sourceUserId,likedUser.Id);

        if(userLike != null)return BadRequest("You already liked this user");

        userLike = new UserLike{

            SourceUserId= sourceUserId,
            LikedUserId = likedUser.Id

        };

        sourceUser.LikedUsers.Add(userLike);

        if(await _unitOfWork.Completed())return Ok();

        return BadRequest("Failed to like user");

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>>  GetUserLikes([FromQuery]LikesParams likesParams){
            
          likesParams.UserId = User.GetUserId();
          var users= await _unitOfWork.likesRepository.GetUserLikes(likesParams);

          Response.AddPaginationHeader(users.CurrentPage,users.PageSize, users.TotalCount, users.TotalPages);

          return Ok(users);
        }

    }
}