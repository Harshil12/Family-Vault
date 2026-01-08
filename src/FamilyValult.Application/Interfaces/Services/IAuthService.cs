using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyVault.Application.Interfaces.Services;

public interface IAuthService
{
    public Task<string?> GetTokenAsync(string email, string password, CancellationToken cancellationToken);
}
