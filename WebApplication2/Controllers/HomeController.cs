using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication2.Models;
using WebApplication2.Models.Repositories; // même using que dans ProductController

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        readonly ICategorieRepository CategRepository;

        public HomeController(ICategorieRepository categRepository)
        {
            this.CategRepository = categRepository;
        }

        public IActionResult Index()
        {
            ViewData["Categories"] = CategRepository.GetAll();
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["Categories"] = CategRepository.GetAll();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}