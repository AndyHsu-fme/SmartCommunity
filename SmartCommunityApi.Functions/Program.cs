using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SmartCommunityApi.Data;
using SmartCommunityApi.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var cfg = context.Configuration;

        services.AddDbContext<SmartCommunityDbContext>(options =>
            options.UseNpgsql(cfg.GetConnectionString("DefaultConnection")));

        var jwtKey = cfg["Jwt:Key"]!;
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = cfg["Jwt:Issuer"],
                    ValidAudience            = cfg["Jwt:Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        services.AddAuthorization();

        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod()));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<INotificationService, NotificationService>();
    })
    .Build();

host.Run();
