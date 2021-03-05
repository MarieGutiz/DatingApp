using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using API.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using API.Interfaces;
using System;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {  //Add it as service in ApplicationServiceExtensions
        public async Task  OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
           var resultContent = await next();

           if(!resultContent.HttpContext.User.Identity.IsAuthenticated) return;

           var userId = resultContent.HttpContext.User.GetUserId();
           var repo = resultContent.HttpContext.RequestServices.GetService<IUserRepository>();
           var user = await repo.GetUserByIdAsync(userId);
           user.LastActive = DateTime.Now;
           await repo.SaveAllAsync();
        }
    }
}