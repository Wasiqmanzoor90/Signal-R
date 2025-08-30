dotnet new webapi -n MyApiProject
cd MyApiProject
dotnet build
dotnet run

dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet tool install --global dotnet-ef

dotnet add package JWT 

dotnet ef migrations add InitialCreate
dotnet ef database update


