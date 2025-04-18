using System;  // Necesario para TimeSpan
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ?? Añadir servicios al contenedor
builder.Services.AddControllersWithViews();

// ?? Configuración de la sesión
builder.Services.AddDistributedMemoryCache();  // Habilitar el almacenamiento de la sesión en memoria
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Configura el tiempo de inactividad
    options.Cookie.HttpOnly = true;  // Protege la cookie de sesión para evitar accesos desde JavaScript
    options.Cookie.IsEssential = true; // Necesario para que funcione sin problemas en producción
});

// ?? Registrar HttpClient para la inyección de dependencias
builder.Services.AddHttpClient();

var app = builder.Build();

// ?? Configuración del pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ?? Activar la sesión en la aplicación (Debe ir después de UseRouting)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
   name: "default",
   pattern: "{controller=Login}/{action=Login" +
   "}/{id?}");

app.Run();
