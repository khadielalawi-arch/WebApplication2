using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using WebApplication2.Models;
using WebApplication2.Models.Repositories;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CategoryController : Controller
    {
        readonly ICategorieRepository CategRepository;

        public CategoryController(ICategorieRepository categRepository)
        {
            CategRepository = categRepository;
        }

        // GET: Category
        [AllowAnonymous]
        public ActionResult Index()
        {
            var Categories = CategRepository.GetAll();
            return View(Categories);
        }

        // GET: Category/Details/5
        public ActionResult Details(int id)
        {
            var category = CategRepository.GetById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category c)
        {
            if (ModelState.IsValid)
            {
                CategRepository.Add(c);
                return RedirectToAction(nameof(Index));
            }
            return View(c);
        }

        // GET: Category/Edit/5
        public ActionResult Edit(int id)
        {
            var category = CategRepository.GetById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Category c)
        {
            if (id != c.CategoryId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                CategRepository.Update(c);
                return RedirectToAction(nameof(Index));
            }
            return View(c);
        }

        // GET: Category/Delete/5
        public ActionResult Delete(int id)
        {
            var category = CategRepository.GetById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CategRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}