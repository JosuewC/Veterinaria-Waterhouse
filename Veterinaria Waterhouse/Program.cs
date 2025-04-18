using System;  // Necesario para TimeSpan
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ?? A�adir servicios al contenedor
builder.Services.AddControllersWithViews();

// ?? Configuraci�n de la sesi�n
builder.Services.AddDistributedMemoryCache();  // Habilitar el almacenamiento de la sesi�n en memoria
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Configura el tiempo de inactividad
    options.Cookie.HttpOnly = true;  // Protege la cookie de sesi�n para evitar accesos desde JavaScript
    options.Cookie.IsEssential = true; // Necesario para que funcione sin problemas en producci�n
});

// ?? Registrar HttpClient para la inyecci�n de dependencias
builder.Services.AddHttpClient();

var app = builder.Build();

// ?? Configuraci�n del pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ?? Activar la sesi�n en la aplicaci�n (Debe ir despu�s de UseRouting)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
   name: "default",
   pattern: "{controller=Login}/{action=Login" +
   "}/{id?}");

app.Run();
