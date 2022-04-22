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
    catch 
    {
        return Results.Unauthorized();
    }
})
.WithName("CurrentBalance");

bool IsScopeValid(string scopeName)
{
    if (ClaimsPrincipal == null)
    {
        Console.WriteLine($"Claims Principal is null");
        return false;
    }

    var scopeClaim = ClaimsPrincipal.HasClaim(x => x.Type == AADConfiguration.ScopeType)
        ? ClaimsPrincipal.Claims.First(x => x.Type == AADConfiguration.ScopeType).Value
        : string.Empty;

    if (string.IsNullOrEmpty(scopeClaim))
    {
        Console.WriteLine($"Scope is invalid: {scopeName}");
        return false;
    }

    if (!scopeClaim.ToUpper().Split(' ').Contains(scopeName.ToUpper()))
    {
        Console.WriteLine($"Scope is invalid: {scopeName}");
        return false;
    }
    Console.WriteLine($"Scope is valid: {scopeName}");
    return true;
}

string? GetUserName()
{
    return ClaimsPrincipal?.Claims.FirstOrDefault(t => t.Type == "preferred_username")?.Value; 
}

app.Run();

