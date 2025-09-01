using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MyApiProject.Extension;

public static class JwtServiceExtenstion
{
    public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["JWT:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("JWT:SecretKey configuration value is missing.");

        }
        var key = Encoding.ASCII.GetBytes(secretKey);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });
        services.AddAuthorization();
        return services;

    }
 }