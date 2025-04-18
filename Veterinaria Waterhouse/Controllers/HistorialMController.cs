using Microsoft.AspNetCore.Mvc;

namespace Veterinaria_Waterhouse.Controllers
{
    public class HistorialMController : Controller
    {
        // Modificado para recibir los parámetros y pasarlos a la vista
        public IActionResult HistorialM(string Servicio, string Fecha)
        {
            // Pasamos los datos a la vista usando ViewBag
            ViewBag.Servicio = Servicio;
            ViewBag.Fecha = Fecha;

            return View();
        }
    }
}
