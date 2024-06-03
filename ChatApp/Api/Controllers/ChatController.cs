using Api.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(ChatService chatService) : ControllerBase
    {
        private ChatService _chatService = chatService;

        [HttpPost("register-uesr")]
        public IActionResult RegisterUser(UserDto model)
        {
            if (_chatService.AddUserToList(model.Name))
                return Ok();
            return BadRequest("This Name Is Taken Please Chose Another Name");
        }
    }
}
