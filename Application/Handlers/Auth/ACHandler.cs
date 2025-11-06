using Application.Commands.Auth;
using Application.Interfaces;
using Application.Security;
using Domain.Models;
using Infrastructure.Application;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Auth
{
    public class ACHandler : IRequestHandler<SignupCommand, string>
    {
        private readonly IUnitOfWork _IUW;
        private readonly IJwtProvider _jwt;

        public ACHandler(IUnitOfWork UOW, IJwtProvider jwt)
        {
            _IUW = UOW;
            _jwt = jwt;
        }

        public async Task<string> Handle(SignupCommand request, CancellationToken cancellationToken)
        {
            var hashedPassword = PasswordHasher.Hash(request.Password);
            var user = new User
            {
                Name = request.FullName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            await _IUW.Users.AddAsync(user);
            await _IUW.CompleteAsync();

            return _jwt.GenerateToken(user);
        }
    }
}
