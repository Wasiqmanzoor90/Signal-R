// Import required namespaces
using Microsoft.EntityFrameworkCore;
using MyApiProject.Inerface;
using MyApiProject.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); // Add this for controller support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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

// Comment out HTTPS redirection for now
// app.UseHttpsRedirection();

// Map controllers
app.MapControllers(); // Add this to map your controllers

// Run the application
app.Run();