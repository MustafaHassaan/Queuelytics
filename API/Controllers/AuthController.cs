using Application.Commands.Auth;
using Infrastructure.Messaging;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IMediator _IM;
        public AuthController(IMediator mediator)
        {
            _IM = mediator;
        }
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupCommand command)
        {
            var token = await _IM.Send(command);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var token = await _IM.Send(command);
            return Ok(new { token });
        }
    }
}
