using Microsoft.AspNetCore.Mvc;

namespace Veterinaria_Waterhouse.Controllers
{
    public class MedicinasController : Controller
    {
        public IActionResult Medicina()
        {
            return View();
        }
    
    }
}
