using Microsoft.AspNetCore.Mvc;

namespace Veterinaria_Waterhouse.Controllers
{
    public class ProductosController : Controller
    {
        public IActionResult ProductosVista()
        {
            return View();
        }
    
    }
}
