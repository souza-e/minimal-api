using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
//using Microsoft.OpenApi.Models;
using minimalAPI.DTOs;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Infraestrutura.Db;




#region Configurao do WebApplication

#region Builder

// esse trecho de cdigo serve para configurar o WebApplication
var builder = WebApplication.CreateBuilder(args);

var Key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(Key)) Key = "123456";


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {

        ValidateLifetime = true,
        //ValidateAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Key)),


    };

});

builder.Services.AddAuthorization();

// esse trecho de cdigo serve para injetar a dependncia do servio de administrador
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta maneira: Bearer {seu token aqui} para autenticar sua requisição"

    });
     /* option.AddSecurityRequirement(new OpenApiSecurityRequirement
       {
           {
               new OpenApiSecurityScheme
               {
                   Reference = new OpenApiReference
                   {
                       // Agora este ID 'Bearer' está definido no Passo 1
                       Type = ReferenceType.SecurityScheme,
                       Id = "Bearer"
                   }
               },
               new string[] {}
           }
       });*/




});

// esse trecho de cdigo serve para injetar a dependncia do DbContexto
builder.Services.AddDbContext<DbContexto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlserver"))


);



#endregion



#region Construoção do App

var app = builder.Build();

#endregion
// esse trecho de cdigo serve para mapear a rota raiz
// o / retorna uma mensagem simples "OLA MUNDO!"
#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

// esse trecho de cdigo serve para mapear a rota de login
// esse endpoint recebe um objeto do tipo LoginDTO no corpo da requisio e usa o servio de administrador para verificar as credenciais
// se as credenciais forem vldas, retorna um status 200 (OK), caso contrrio, retorna um status 401 (Unauthorized)
// o /login  espera um JSON no seguinte formato:
// {"email": "  
// "password": " "}
#region Administradores


string GerarTokenJwt(Administrador administrador)
{
    if (string.IsNullOrEmpty(Key)) return string.Empty;
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil)
    };

    var token = new JwtSecurityToken(
       claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );
    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    var adm = administradorServico.Login(loginDTO);
    if (adm != null)
    {
        string token = GerarTokenJwt(adm);

        return Results.Ok(new AdmLogado
        {
            // Email = adm.Email,
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }


    else
        return Results.Unauthorized();
}).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{

    var administrador = administradorServico.BuscarPorId(id);

    if (administrador == null) return Results.NotFound();

    return Results.Ok(new AdministradorModelView
    {
        id = administrador.Id,
        email = administrador.Email,
        perfil = administrador.Perfil
    });



}).RequireAuthorization().WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var admins = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina);

    foreach (var admin in administradores)
    {
        admins.Add(new AdministradorModelView
        {
            id = admin.Id,
            email = admin.Email,
            perfil = admin.Perfil
        });
    }

    return Results.Ok(admins);

}).RequireAuthorization().WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = new ErrosDeValidacao { Mensagem = new List<string>() };

    if (string.IsNullOrEmpty(administradorDTO.email))
    {
        validacao.Mensagem.Add("O email do administrador no pode ser vazio");
    }
    if (string.IsNullOrEmpty(administradorDTO.password))
    {
        validacao.Mensagem.Add("Senha não pode ser vazia");
    }
    if (administradorDTO.perfil == null)
    {
        validacao.Mensagem.Add("Perfil não pode ser vazio");
    }

    if (validacao.Mensagem.Count > 0)
    {
        return Results.BadRequest(validacao);
    }



    var administrador = new Administrador
    {
        Email = administradorDTO.email,
        Senha = administradorDTO.password,
        Perfil = administradorDTO.perfil.ToString() ?? Perfil.Editor.ToString()
    };
    administradorServico.Incluir(administrador);

    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelView
    {
        id = administrador.Id,
        email = administrador.Email,
        perfil = administrador.Perfil
    });


}).RequireAuthorization().WithTags("Administradores");





#endregion


#region  veiculosPOST

ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{

    var Validacao = new ErrosDeValidacao { Mensagem = new List<string>() };

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
    {
        Validacao.Mensagem.Add("O nome do veiculo no pode ser vazio");
    }

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
    {
        Validacao.Mensagem.Add("A marca do veiculo não pode ficar em branco");
    }
    if (veiculoDTO.Ano < 1950)
    {
        Validacao.Mensagem.Add("Veiuclo muito antigo, só aceito a partir de 1950");
    }
    return Validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{



    var Validacao = validaDTO(veiculoDTO);

    if (Validacao.Mensagem.Count > 0)
    {
        return Results.BadRequest(Validacao);
    }


    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Adicionar(veiculo);
    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);

}).RequireAuthorization().WithTags("Veiculos");

#endregion


#region  veiculosGET

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{

    var veiculos = veiculoServico.Todos(pagina);
    return Results.Ok(veiculos);



}).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{

    var veiculos = veiculoServico.BuscarPorId(id);

    if (veiculos == null) return Results.NotFound();

    return Results.Ok(veiculos);



}).RequireAuthorization().WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    var veiculos = veiculoServico.BuscarPorId(id);
    if (veiculos == null) return Results.NotFound();


    var Validacao = validaDTO(veiculoDTO);
    if (Validacao.Mensagem.Count > 0)
        return Results.BadRequest(Validacao);





    veiculos.Nome = veiculoDTO.Nome;
    veiculos.Marca = veiculoDTO.Marca;
    veiculos.Ano = veiculoDTO.Ano;


    veiculoServico.Atualizar(veiculos);
    return Results.Ok(veiculos);



}).RequireAuthorization().WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{

    var veiculos = veiculoServico.BuscarPorId(id);

    if (veiculos == null) return Results.NotFound();


    veiculoServico.Deletar(veiculos);
    return Results.NoContent();



}).RequireAuthorization().WithTags("Veiculos");
#endregion
#region App


app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();



#endregion


app.Run();


#endregion