using Microsoft.EntityFrameworkCore;
using MyApiProject.Inerface;
using MyApiProject.Service;
using MyApiProject.Model;
using System.Text.Json.Serialization;
using MyApiProject.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJwt(builder.Configuration);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<MessageService>();
builder.Services.AddDbContext<SqlDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// JSON token service
builder.Services.AddScoped<IJsonToken, JsonTokenService>();

// 1. Add SignalR services
builder.Services.AddSignalR();

// CORS configuration for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials() // This is crucial for SignalR
               .WithExposedHeaders("WWW-Authenticate"); // sometimes needed
    });
});

var app = builder.Build();

// Configure the middleware (HTTP request pipeline)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        options.RoutePrefix = string.Empty; // this makes Swagger UI load at "/"
    });
}

// IMPORTANT: CORS must be before Authentication and Authorization
app.UseCors("SignalRPolicy");

app.UseAuthentication(); // **must be before UseAuthorization**

app.UseAuthorization();

// Map controllers
app.MapControllers();

// 2. Map ChatHub endpoint with CORS
app.MapHub<ChatHub>("/chathub");

// Run the application
app.Run();