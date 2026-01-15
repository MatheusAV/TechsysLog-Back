using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using TechsysLog.Api.Middlewares;
using TechsysLog.Application.Services;
using TechsysLog.Application.Services.Interfaces;
using TechsysLog.Infrastructure;
using TechsysLog.Infrastructure.Auth;
using TechsysLog.Infrastructure.Persistence.Mongo;
using TechsysLog.Realtime;
using TechsysLog.Realtime.Hubs;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;


var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TechsysLog API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Informe: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Infrastructure + Realtime
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRealtime();

// JWT Auth
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
          ?? throw new InvalidOperationException("Config Jwt inválida.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.RequireHttpsMetadata = false;
       options.SaveToken = true;
       options.MapInboundClaims = false;
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateIssuerSigningKey = true,
           ValidateLifetime = true,
           ValidIssuer = jwt.Issuer,
           ValidAudience = jwt.Audience,
           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
           ClockSkew = TimeSpan.FromMinutes(2),
           NameClaimType = JwtRegisteredClaimNames.Sub
       };

       options.Events = new JwtBearerEvents
       {
           OnMessageReceived = context =>
           {
               var accessToken = context.Request.Query["access_token"];
               var path = context.HttpContext.Request.Path;

               if (!string.IsNullOrWhiteSpace(accessToken) &&
                   path.StartsWithSegments("/hubs/notifications"))
               {
                   context.Token = accessToken;
               }

               return Task.CompletedTask;
           },

           OnAuthenticationFailed = context =>
           {
               Console.WriteLine("[JWT DEBUG] AuthenticationFailed: " + context.Exception);
               return Task.CompletedTask;
           },

           OnChallenge = context =>
           {
               Console.WriteLine("[JWT DEBUG] Challenge error: " + context.Error);
               Console.WriteLine("[JWT DEBUG] Challenge desc : " + context.ErrorDescription);
               return Task.CompletedTask;
           },

           OnTokenValidated = context =>
           {
               Console.WriteLine("[JWT DEBUG] TokenValidated");
               return Task.CompletedTask;
           }
       };
   });





builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEnd", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});



builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware de exceção
app.UseMiddleware<ExceptionMiddleware>();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

//Cors
app.UseCors("FrontEnd");

// Auth
app.Use(async (ctx, next) =>
{
    var auth = ctx.Request.Headers.Authorization.ToString();
    Console.WriteLine($"Authorization Header: {auth}");
    await next();
});
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// SignalR Hub
app.MapHub<NotificationHub>("/hubs/notifications");

// Inicializa índices do Mongo
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IMongoIndexInitializer>();
    await initializer.EnsureIndexesAsync(CancellationToken.None);
}

app.Run();

public partial class Program { }