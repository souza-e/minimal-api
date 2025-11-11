var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "OLA eeeMUNDO!");



app.MapPost("/login", (minimalAPI.DTOs.LoginDTO loginDTO) =>
{
    if (loginDTO.email == "admin@teste.com" && loginDTO.password == "123456")
        return Results.Ok("Login realizado com sucesso");
    else
        return Results.Unauthorized();
});


app.Run();


