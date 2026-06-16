// using Microsoft.EntityFrameworkCore;
// using Concesionario.Data;
// using Microsoft.Extensions.FileProviders;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;
// using System.Text;
// using System.Globalization; // Requerido para la localización

// var builder = WebApplication.CreateBuilder(new WebApplicationOptions
// {
//     Args = args,
//     WebRootPath = "wwwroot"
// });

// // 1. Configuración de la Base de Datos
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseMySQL(connectionString));

// // --- CONFIGURACIÓN DE SEGURIDAD JWT ---
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

//     // MANEJO DE REDIRECCIÓN: Evita el 404 al navegar manualmente
//     options.Events = new JwtBearerEvents
//     {
//         OnChallenge = context =>
//         {
//             // Si la petición viene del navegador (pide HTML), redirigimos al login de forma absoluta
//             if (context.Request.Headers["Accept"].ToString().Contains("text/html"))
//             {
//                 context.HandleResponse();
//                 context.Response.Redirect("/Admin/Index"); 
//             }
//             return Task.CompletedTask;
//         }
//     };
// });

// builder.Services.AddControllersWithViews();

// // --- CONFIGURACIÓN GLOBAL DE CULTURA (SOLUCIÓN SIGNO DE PESOS) ---
// var supportedCultures = new[] { "es-AR" };
// var localizationOptions = new RequestLocalizationOptions()
//     .SetDefaultCulture(supportedCultures[0])
//     .AddSupportedCultures(supportedCultures)
//     .AddSupportedUICultures(supportedCultures);

// builder.Services.Configure<RequestLocalizationOptions>(options =>
// {
//     options.DefaultRequestCulture = localizationOptions.DefaultRequestCulture;
//     options.SupportedCultures = localizationOptions.SupportedCultures;
//     options.SupportedUICultures = localizationOptions.SupportedUICultures;
// });

// var app = builder.Build();

// // --- ACTIVACIÓN DEL MIDDLEWARE DE LOCALIZACIÓN ---
// app.UseRequestLocalization(localizationOptions);

// // 2. Pipeline de configuración
// if (app.Environment.IsDevelopment())
// {
//     app.UseDeveloperExceptionPage(); // Te muestra errores detallados en desarrollo
// }
// else
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

// // --- ORDEN CRÍTICO DE MIDDLEWARES ---
// app.UseAuthentication(); // 1. Identifica al usuario (JWT / Cookies)
// app.UseAuthorization();  // 2. Valida sus permisos

// app.MapStaticAssets();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

// // --- AUTOMIGRACIÓN EN LA NUBE ---
// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     try
//     {
//         var context = services.GetRequiredService<ApplicationDbContext>();
//         if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
//         {
//             context.Database.Migrate();
//         }
//     }
//     catch (Exception ex)
//     {
//         var logger = services.GetRequiredService<ILogger<Program>>();
//         logger.LogError(ex, "Un error ocurrió al aplicar las migraciones en TiDB Cloud.");
//     }
// }

// app.Run();





using Microsoft.EntityFrameworkCore;
using Concesionario.Data;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Globalization; 

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

    // MANEJO DE REDIRECCIÓN Y RESPUESTAS DE API
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Evitamos que ASP.NET maneje la respuesta por defecto
            context.HandleResponse();

            // CASO A: Si es una petición del navegador buscando una página HTML, redirigimos al login
            if (context.Request.Headers["Accept"].ToString().Contains("text/html"))
            {
                context.Response.Redirect("/Home/Acceso"); 
            }
            // CASO B: Si es un Fetch / AJAX de JS buscando datos (JSON), devolvemos 401 limpio
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var resultado = System.Text.Json.JsonSerializer.Serialize(new { mensaje = "Token inválido o expirado. Inicie sesión nuevamente." });
                return context.Response.WriteAsync(resultado);
            }
            
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllersWithViews();

// --- CONFIGURACIÓN GLOBAL DE CULTURA (SOLUCIÓN SIGNO DE PESOS) ---
var supportedCultures = new[] { "es-AR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = localizationOptions.DefaultRequestCulture;
    options.SupportedCultures = localizationOptions.SupportedCultures;
    options.SupportedUICultures = localizationOptions.SupportedUICultures;
});

var app = builder.Build();

// --- ACTIVACIÓN DEL MIDDLEWARE DE LOCALIZACIÓN ---
app.UseRequestLocalization(localizationOptions);

// 2. Pipeline de configuración
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); 
}
else
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
app.UseAuthentication(); // 1. Identifica al usuario (JWT / Cookies)
app.UseAuthorization();  // 2. Valida sus permisos

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- AUTOMIGRACIÓN EN LA NUBE ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Un error ocurrió al aplicar las migraciones en TiDB Cloud.");
    }
}

app.Run();