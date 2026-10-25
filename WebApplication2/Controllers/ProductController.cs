using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication2.Models;
using WebApplication2.Models.Repositories;
using WebApplication2.ViewModels;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProductController : Controller
    {
        readonly IProductRepository ProductRepository;
        readonly ICategorieRepository CategRepository;
        private readonly IWebHostEnvironment hostingEnvironment;

        public ProductController(IProductRepository ProdRepository,
                                 ICategorieRepository categRepository,
                                 IWebHostEnvironment hostingEnvironment)
        {
            ProductRepository = ProdRepository;
            CategRepository = categRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: Product  (filtre par catégorie + pagination)
        [AllowAnonymous]
        public IActionResult Index(int? categoryId, int page = 1)
        {
            int pageSize = 4; // Nombre de produits par page
            if (page < 1) page = 1;

            // Catégories pour le menu latéral
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

            // Produits filtrés par catégorie si categoryId est spécifié
            IQueryable<Product> productsQuery = ProductRepository.GetAllProducts();
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            // Pagination
            var totalProducts = productsQuery.Count();
            var products = productsQuery
                            .OrderBy(p => p.ProductId)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.SelectedCategoryId = categoryId; // conserve le filtre dans la pagination

            return View(products);
        }

        // GET: Product/Details/5
        public ActionResult Details(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

            var product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();
            return View(product);
        }

        // GET: Product/Create
        public ActionResult Create()
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName", model.CategoryId);

            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                // Si une image a été choisie, on l'enregistre dans wwwroot/images
                if (model.ImagePath != null)
                {
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadsFolder); // crée le dossier s'il n'existe pas

                    // Nom unique : GUID + "_" + nom d'origine
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // using : ferme le fichier une fois la copie terminée
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ImagePath.CopyTo(fileStream);
                    }
                }

                Product newProduct = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    QteStock = model.QteStock,
                    CategoryId = model.CategoryId,
                    Image = uniqueFileName   // nom du fichier enregistré en base
                };

                ProductRepository.Add(newProduct);
                return RedirectToAction("Details", new { id = newProduct.ProductId });
            }

            // Formulaire invalide : on réaffiche avec la liste des catégories
            return View(model);
        }

        // GET: Product/Edit/5
        public ActionResult Edit(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName");

            Product product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();

            EditViewModel productEditViewModel = new EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                QteStock = product.QteStock,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.Image
            };
            return View(productEditViewModel);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName");

            // Si les données sont invalides, on réaffiche le formulaire
            if (ModelState.IsValid)
            {
                // Récupérer le produit à modifier
                Product product = ProductRepository.GetById(model.ProductId);
                if (product == null)
                    return NotFound();

                // Mettre à jour les champs
                product.Name = model.Name;
                product.Price = model.Price;
                product.QteStock = model.QteStock;
                product.CategoryId = model.CategoryId;

                // Si une nouvelle image est envoyée, on supprime l'ancienne
                // puis on enregistre la nouvelle dans wwwroot/images
                if (model.ImagePath != null)
                {
                    if (model.ExistingImagePath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingImagePath);
                        System.IO.File.Delete(filePath);
                    }
                    product.Image = ProcessUploadedFile(model);
                }

                Product updatedProduct = ProductRepository.Update(product);
                if (updatedProduct != null)
                    return RedirectToAction("Index");
                else
                    return NotFound();
            }
            return View(model);
        }

        [NonAction]
        private string ProcessUploadedFile(EditViewModel model)
        {
            string uniqueFileName = null;
            if (model.ImagePath != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImagePath.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        // GET: Product/Delete/5
        public ActionResult Delete(int id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

            var product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();
            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Product/ByCategory/2
        public ActionResult ByCategory(int? id)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

            var result = ProductRepository.GetProductsByCategID(id);
            return View("Index", result);
        }

        // GET: Product/Search?val=...
        public ActionResult Search(string val)
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;

            var result = ProductRepository.FindByName(val);
            return View("Index", result);
        }
    }
}