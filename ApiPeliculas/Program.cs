using ApiPeliculas.Data;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ApiPeliculas.PeliculasMappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ApiPeliculas.Modelos;

var builder = WebApplication.CreateBuilder(args);

// Configuramos la conexi�n a sql server
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
{
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql"));
});

// Soporte para autenticaci�n con .NET Identity
builder.Services.AddIdentity<AppUsuario, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

// A�adimos Cach�
builder.Services.AddResponseCaching();

// Agregamos los repositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();


var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

// Agregar el automapper
builder.Services.AddAutoMapper(typeof(PeliculasMapper));

// Aqu� se configura la Autenticaci�n
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});


// Add services to the container.

builder.Services.AddControllers(opcion =>
{
    // Cache profile. Un cach� global
    opcion.CacheProfiles.Add("PorDefecto20Segundos", new CacheProfile()
    {
        Duration = 20
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticaci�n JWT usando el esquema Bearer. \r\n\r\n " +
        "Ingresa la palabra 'Bearer' seguida de un [espacio] y despu�s su token en el campo de abajo \r\n\r\n" +
        "Ejemplo: \"Bearer adsodksoakdsad\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In= ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Soporte para CORS
// Se pueden habilitar: 1 - Un dominio, 2 - multiples dominios,
// 3 - cualquier dominio (Tener en cuenta seguridad)
// Usamos de ejemplo el dominio: http://localhost:3223, se debe cambiar por el correcto
// Se usa (*) para todos los dominios
builder.Services.AddCors(p=> p.AddPolicy("PolicyCors", build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Soporte para CORS
app.UseCors("PolicyCors");

// Autenticaci�n
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
