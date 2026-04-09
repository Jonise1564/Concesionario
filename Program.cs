// using Microsoft.EntityFrameworkCore;
// using Concesionario.Data;
// using Microsoft.Extensions.FileProviders;
// using Microsoft.AspNetCore.Authentication.JwtBearer; // Necesario
// using Microsoft.IdentityModel.Tokens;               // Necesario
// using System.Text;

// var builder = WebApplication.CreateBuilder(new WebApplicationOptions
// {
//     Args = args,
//     WebRootPath = "wwwroot"
// });

// // 1. Configuración de la Base de Datos
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseMySQL(connectionString));

// // --- NUEVO: CONFIGURACIÓN DE SEGURIDAD JWT ---
// var jwtSettings = builder.Configuration.GetSection("JWT");
// var secretKey = Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("Secret"));

// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = jwtSettings.GetValue<string>("ValidIssuer"),
//         ValidAudience = jwtSettings.GetValue<string>("ValidAudience"),
//         IssuerSigningKey = new SymmetricSecurityKey(secretKey)
//     };
// });
// // ----------------------------------------------

// builder.Services.AddControllersWithViews();

// var app = builder.Build();

// // 2. Pipeline de configuración
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     app.UseHsts();
// }

// app.UseHttpsRedirection();

// // Solución para archivos estáticos (wwwroot)
// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new PhysicalFileProvider(
//         Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
//     RequestPath = ""
// });

// app.UseRouting();

// // --- CRITICO: ORDEN DE LOS MIDDLEWARES ---
// app.UseAuthentication(); // 1. ¿Quién es el usuario? (JWT)
// app.UseAuthorization();  // 2. ¿Qué puede hacer el usuario?
// // -----------------------------------------

// app.MapStaticAssets();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

// app.Run();



using Microsoft.EntityFrameworkCore;
using Concesionario.Data;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot"
});

// 1. Configuración de la Base de Datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));

// --- CONFIGURACIÓN DE SEGURIDAD JWT ---
var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("Secret"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("ValidIssuer"),
        ValidAudience = jwtSettings.GetValue<string>("ValidAudience"),
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };

    // MANEJO DE REDIRECCIÓN: Evita el 404 al navegar manualmente
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Si la petición viene del navegador (pide HTML), redirigimos al login
            if (context.Request.Headers["Accept"].ToString().Contains("text/html"))
            {
                context.HandleResponse();
                context.Response.Redirect("/Home/Acceso");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 2. Pipeline de configuración
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Solución para archivos estáticos (wwwroot)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = ""
});

app.UseRouting();

// --- ORDEN CRÍTICO DE MIDDLEWARES ---
app.UseAuthentication(); // 1. Identifica al usuario
app.UseAuthorization();  // 2. Valida sus permisos

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();