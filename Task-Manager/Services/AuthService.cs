using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Task_Manager.Models;

namespace Task_Manager.Services
{
    public class AuthService
    {
        public string GenerateToken(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Configuration.SecretKey);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = GenerateClaims(user),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = credentials
            };

            var token = handler.CreateToken(tokenDescriptor);
            var tokenString = handler.WriteToken(token);
            return tokenString;
        }

        public ClaimsIdentity GenerateClaims(User user)
        {
            var ci = new ClaimsIdentity();
            ci.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            ci.AddClaim(new Claim(ClaimTypes.Role, user.Role));

            return ci;
        }

        public bool ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Configuration.SecretKey);
            try
            {
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetHashedPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = GetHashedPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}
