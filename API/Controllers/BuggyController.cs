using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BuggyController : BaseApiControler
    {
        private readonly DataContext _context;
        public BuggyController(DataContext context)
        {
            this._context = context;
        }

        [Authorize]
        [HttpGet("Auth")]
        public ActionResult<string>  GetSecret(){

              return "secret text";
        }

       
        [HttpGet("not-found")]
        public ActionResult<AppUser>  GetNotFound(){
           var user= _context.Users.Find(-1);

           if(user ==null)return NotFound();
            
            return Ok(user);
        }

       
        [HttpGet("server-error")]
        public ActionResult<string>  GetServerError(){
           
             var user= _context.Users.Find(-1);
             var userToReturn = user.ToString();
              return userToReturn;          
          
        }

       
        [HttpGet("bad-request")]
        public ActionResult<AppUser>  GetBadRequest(){
            //   return BadRequest("This was not a good request");
             return BadRequest();
        }
       
    }
}