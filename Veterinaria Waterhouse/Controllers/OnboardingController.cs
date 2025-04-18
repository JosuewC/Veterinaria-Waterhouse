using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;

namespace Veterinaria.Controllers
{
    public class OnboardingController : Controller
    {
        public IActionResult pantalla1()
        {
            // Limpiar las cookies automáticamente antes de iniciar el Onboarding
            LimpiarCookies();

            // Permitir siempre el acceso a pantalla1
            return View();
        }

        public IActionResult pantalla2()
        {
            // Permitir siempre el acceso a pantalla2
            return View();
        }

        public IActionResult pantalla3()
        {
            // Permitir siempre el acceso a pantalla3
            return View();
        }

        public IActionResult CompletarOnboarding()
        {
            Response.Cookies.Append("OnboardingCompleted", "true", new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                Path = "/" // Importante para asegurarse de que se aplique globalmente
            });

            return RedirectToAction("Login", "Login");
        }

        private void LimpiarCookies()
        {
            // Elimina cookies específicas relacionadas con el Onboarding
            var cookiesAEliminar = new[] { "OnboardingCompleted", "pantalla1", "pantalla2", "pantalla3" };

            foreach (var cookie in cookiesAEliminar)
            {
                if (Request.Cookies.ContainsKey(cookie))
                {
                    Response.Cookies.Delete(cookie, new CookieOptions
                    {
                        Path = "/"  // Asegúrate de que se eliminen globalmente
                    });
                }
            }

            // Fuerza la expiración de las cookies por si acaso
            foreach (var cookie in cookiesAEliminar)
            {
                Response.Cookies.Append(cookie, "", new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Path = "/" // Asegúrate de que se eliminen globalmente
                });
            }
        }
    }
}
