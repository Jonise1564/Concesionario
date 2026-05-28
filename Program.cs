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
                context.Response.Redirect("Admin/Index");
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

// --- AUTOMIGRACIÓN EN LA NUBE ---
// Este bloque revisa TiDB Cloud al arrancar y crea las tablas si no existen
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