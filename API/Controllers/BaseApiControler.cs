using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]//to get last activity
     [ApiController]
    [Route("api/[controller]")]
    public class BaseApiControler : ControllerBase
    {
        
    }
}