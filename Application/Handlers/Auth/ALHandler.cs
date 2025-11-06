using Application.Commands.Auth;
using Application.Interfaces;
using Application.Security;
using Infrastructure.Application;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Auth
{
    public class ALHandler : IRequestHandler<LoginCommand, string>
    {
        private readonly IUnitOfWork _IUW;
        private readonly IJwtProvider _jwt;
        private readonly IRedisCacheService _cache;

        public ALHandler(IUnitOfWork UOW, IJwtProvider jwt, IRedisCacheService cache)
        {
            _IUW = UOW;
            _jwt = jwt;
            _cache = cache;
        }
        public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _IUW.Users.GetByEmailAsync(request.Email);
            if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            var token = _jwt.GenerateToken(user);

            await _cache.SetAsync($"jwt:{user.Id}", token, TimeSpan.FromMinutes(120));

            return token;
        }
    }
}
