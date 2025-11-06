using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Security
{
    public interface IJwtProvider
    {
        string GenerateToken(User user);
    }
}
