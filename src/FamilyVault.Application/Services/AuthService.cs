using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FamilyVault.Application.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly ICryptoService _cryptoService;

    public AuthService(IConfiguration config, IUserRepository userRepository, IMapper mapper, ICryptoService cryptoService)
    {
        _config = config;
        _mapper = mapper;
        _userRepository = userRepository;
        _cryptoService = cryptoService;
    }

    public async Task<string?> GetTokenAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user == null)
            return null;

        if (!_cryptoService.VerifyPassword(user.Password, password)) // In real scenarios, use hashed password comparison
            return null;

        var useDto = _mapper.Map<UserDto>(user);

        return GenerateToken(useDto);
    }

    public string GenerateToken(UserDto user)
    {
        var jWTSettingsSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jWTSettingsSection["Key"] ?? string.Empty));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name,$"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jWTSettingsSection["Issuer"],
            audience: jWTSettingsSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
