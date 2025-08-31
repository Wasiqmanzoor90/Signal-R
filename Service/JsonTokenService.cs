using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JWT.Builder;
using Microsoft.IdentityModel.Tokens;
using MyApiProject.Inerface;

namespace MyApiProject.Service
{
    public class JsonTokenService : IJsonToken
    {
        private readonly string _secretkey;

        public JsonTokenService(IConfiguration configuration)
        {
            _secretkey = configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is missing in configuration.");
        }
        public string CreateToken(Guid Id, string Name, string Email)
        {

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretkey);
                var tokendescription = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity([
                        new Claim(ClaimTypes.NameIdentifier, Id.ToString()),
                        new Claim(ClaimTypes.Name, Name),
                        new Claim(ClaimTypes.Email, Email)
                    ]),
                    Expires = DateTime.UtcNow.AddHours(10),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokendescription);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {

                throw new Exception("Server error: " + ex.Message);
            }
        }

        public Guid VerifyToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretkey);
                var validtoken = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                 var principal = tokenHandler.ValidateToken(token, validtoken, out var validatedToken);
            var useridclaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (useridclaim != null)
            {
                return new Guid(useridclaim.Value);
            }

            else
            {
                throw new Exception("User ID not found in token.");


            }
            }
            catch (Exception ex)
            {

                throw new Exception("Server error: " + ex.Message);
            }



         }

    }
    
}