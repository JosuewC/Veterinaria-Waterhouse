using Microsoft.AspNetCore.Mvc;

namespace Veterinaria_Waterhouse.Controllers
{
    public class MenuController : Controller
    {
        public IActionResult Menu()
        {
            return View();
        }
    
    }
}
