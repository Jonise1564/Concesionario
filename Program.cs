// using Microsoft.EntityFrameworkCore;
// using Concesionario.Data;

// var builder = WebApplication.CreateBuilder(args);

// // 1. Configuración de la Base de Datos (MySQL)
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseMySQL(connectionString));

// // 2. Agregar servicios al contenedor
// builder.Services.AddControllersWithViews();

// var app = builder.Build();

// // 3. Configurar el pipeline de solicitudes HTTP
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     app.UseHsts();
// }

// app.UseHttpsRedirection();


// app.UseStaticFiles(); 

// app.UseRouting();
// app.UseAuthorization();


// app.MapStaticAssets();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

// app.Run();


using Microsoft.EntityFrameworkCore;
using Concesionario.Data;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot" // Forzamos el nombre de la carpeta web
});

// 1. Configuración de la Base de Datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 2. Pipeline de configuración
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// --- SOLUCIÓN DEFINITIVA PARA LAS FOTOS ---
// Forzamos al servidor a leer físicamente la carpeta wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = ""
});
// ------------------------------------------

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();