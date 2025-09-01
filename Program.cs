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

//json token servoce
// Program.cs - This runs ONCE when app starts
builder.Services.AddScoped<IJsonToken, JsonTokenService>();

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

app.UseAuthentication(); // **must be before UseAuthorization**
app.UseAuthorization();
// Map controllers
app.MapControllers(); // Add this to map your controllers

// 1. Add SignalR services
builder.Services.AddSignalR();

// 2. Map ChatHub endpoint
app.MapHub<ChatHub>("/chathub");


// Run the application
app.Run();