using MathApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABASE CONFIGURATION
// We use Environment Variables so Docker can tell the API where the SQL Server is
builder.Services.AddDbContext<MathDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("Math_DB")));

// 2. JWT KEY CONFIGURATION
// We pull the secret key from the Docker environment to sign our "Badges"
var jwtKey = Environment.GetEnvironmentVariable("MathAppJwtKey");
var keyBytes = Encoding.ASCII.GetBytes(jwtKey ?? "Default_Secret_Key_At_Least_32_Chars");

// 3. AUTHENTICATION SERVICES
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in real production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,   // Set to true if you want to validate the sender
        ValidateAudience = false  // Set to true if you want to validate the receiver
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. MIDDLEWARE PIPELINE (ORDER MATTERS!)
if (app.Environment.IsDevelopment())
{
   app.UseSwagger(); 
   app.UseSwaggerUI(); 
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
// UseAuthentication MUST come before UseAuthorization
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();