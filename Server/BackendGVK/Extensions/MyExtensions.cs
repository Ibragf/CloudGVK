using BackendGVK.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Neo4jClient;
using System.Text;

namespace BackendGVK.Extensions
{
    public static class MyExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, string issuer, string audience, string secretKey)
        {
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = signInKey,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }

        public static IApplicationBuilder UseTokenManager(this IApplicationBuilder app)
        {
            app.UseMiddleware<TokenManagerMiddleware>();
            return app;
        }

        public static async Task CreateConstraintsIfNotExistsAsync(this BoltGraphClient boltClient)
        {
            string userType = "User";
            string dirType = ElementTypes.Directory.ToString();
            string fileType = ElementTypes.File.ToString();
            string invitationType = "INVITED";

            var graphConstraints = new GraphConstraints(boltClient.Cypher);

            graphConstraints.AddUniqueConstraint(userType, nameof(ApplicationUser.Email));
            graphConstraints.AddUniqueConstraint(userType, nameof(ApplicationUser.Id));

            graphConstraints.AddUniqueConstraint(invitationType, nameof(InvitationModel.Id));

            graphConstraints.AddUniqueConstraint(dirType, nameof(DirectoryModel.Id));

            graphConstraints.AddUniqueConstraint(fileType, nameof(FileModel.Id));
            graphConstraints.AddUniqueConstraint(fileType, nameof(FileModel.CrcHash));
            graphConstraints.AddUniqueConstraint(fileType, nameof(FileModel.TrustedName));

            await graphConstraints.ExecuteAsync();
        }
    }
}
