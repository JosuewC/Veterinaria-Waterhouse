using Microsoft.AspNetCore.Mvc;

namespace Veterinaria_Waterhouse.Controllers
{
    public class InicioController : Controller

    {
        [HttpGet]
        public IActionResult inicio()
        {
            return View();
        }
    
    }
}
