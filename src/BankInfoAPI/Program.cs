using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using BankInfoAPI;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
Random random = new();
ClaimsPrincipal? ClaimsPrincipal;


app.MapGet("/currentBalance", (HttpContext context) =>
{
    string bearerToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ","");
    if (string.IsNullOrWhiteSpace(bearerToken)) return Results.Unauthorized();
    var tokenValidator = new JwtSecurityTokenHandler();
    var validationParameters = new TokenValidationParameters
    {
        RequireSignedTokens = true,
        ValidAudience = AADConfiguration.BankInfoAPIClientID,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        IssuerSigningKeys = AADConfiguration.OpenIdConnectConfiguration?.SigningKeys,
        ValidIssuer = AADConfiguration.OpenIdConnectConfiguration?.Issuer
    };
    try
    {
        SecurityToken securityToken;
        ClaimsPrincipal = tokenValidator.ValidateToken(bearerToken, validationParameters, out securityToken);

        if (IsScopeValid(AADConfiguration.BankInfoAPIScope))
        {
            return Results.Ok(string.Format("Hello {0}, your Balance is: {1}", GetUserName(), random.NextInt64(-100000, 100000))); ;        
        }
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        Console.Write("Access token validation error: {0}", ex.Message);
        return Results.Unauthorized();
    }
})
.WithName("CurrentBalance");

bool IsScopeValid(string scopeName)
{
    if (ClaimsPrincipal == null)
    {
        Console.WriteLine($"Access token validation error: claims principal is null");
        return false;
    }

    var scopeClaim = ClaimsPrincipal.HasClaim(x => x.Type == AADConfiguration.ScopeType)
        ? ClaimsPrincipal.Claims.First(x => x.Type == AADConfiguration.ScopeType).Value
        : string.Empty;

    if (string.IsNullOrEmpty(scopeClaim))
    {
        Console.WriteLine($"Access token validation error: {scopeName} is invalid");
        return false;
    }

    if (!scopeClaim.ToUpper().Split(' ').Contains(scopeName.ToUpper()))
    {
        Console.WriteLine($"Access token validation error: {scopeName} is invalid");
    }
    Console.WriteLine($"Access token scope {scopeName} is valid");
    return true;
}

string? GetUserName()
{
    return ClaimsPrincipal?.Claims.FirstOrDefault(t => t.Type == "name")?.Value; 
}

app.Run();

