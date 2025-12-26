using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Carregar configuração do Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Configurar JWT
// Em produção, a chave deve vir de variável de ambiente
var secretKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKey12345678901234567890";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddOcelot(builder.Configuration);


var app = builder.Build();

app.UseRouting();

// Health Check simples
app.MapGet("/", () => "Api Gateway is running!");

// Endpoints de Autenticação (Auth Server Embutido)
app.MapPost("/auth/token", (UserLogin user) =>
{
    // Simulação de validação de usuário
    if (user.Username == "admin" && user.Password == "admin")
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "Admin")
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);
        var refreshToken = Guid.NewGuid().ToString();

        // TODO: Salvar refreshToken no banco de dados

        return Results.Ok(new { Token = jwtToken, RefreshToken = refreshToken });
    }
    return Results.Unauthorized();
});

app.MapPost("/auth/refresh-token", (RefreshTokenRequest request) =>
{
    // Simulação de validação de refresh token
    if (!string.IsNullOrEmpty(request.RefreshToken))
    {
        // Validar se o refresh token existe e é válido no banco

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "admin")
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);
        var newRefreshToken = Guid.NewGuid().ToString();

        return Results.Ok(new { Token = jwtToken, RefreshToken = newRefreshToken });
    }
    return Results.BadRequest("Invalid Refresh Token");
});


app.UseAuthentication();
app.UseAuthorization();

// Endpoints próprios (MapGet, MapPost) já definidos acima

await app.UseOcelot();

app.Run();

record UserLogin(string Username, string Password);
record RefreshTokenRequest(string RefreshToken);
