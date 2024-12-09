using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductCategoryApp.Data;
using ProductCategoryApp.Models;

namespace ProductCategoryApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            // Fetch products with the correct pagination.
            var products = _context.Products.Include(p => p.Category)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Calculate the total number of records in the database (used for pagination).
            var totalRecords = _context.Products.Count();

            // Create a PaginatedProductViewModel to send the necessary data to the view.
            var viewModel = new PaginatedProductViewModel
            {
                Items = products,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                CurrentPage = page
            };

            return View(viewModel);
        }



        // Display Create Product View
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // Handle Create Product form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product, string newCategoryName)
        {
            if (ModelState.IsValid)
            {
                // Handle new category creation or existing category selection
                if (!string.IsNullOrEmpty(newCategoryName))
                {
                    // Check if the category already exists
                    var existingCategory = _context.Categories
                        .FirstOrDefault(c => c.CategoryName.Equals(newCategoryName, StringComparison.OrdinalIgnoreCase));

                    if (existingCategory == null)
                    {
                        // Create new category if it doesn't exist
                        var newCategory = new Category
                        {
                            CategoryName = newCategoryName
                        };
                        _context.Categories.Add(newCategory);
                        _context.SaveChanges();  // Save changes to ensure the new category gets an ID

                        // Associate the new category with the product
                        product.CategoryId = newCategory.CategoryId;
                    }
                    else
                    {
                        // If the category exists, associate it with the product
                        product.CategoryId = existingCategory.CategoryId;
                    }
                }

                // Add the product to the database
                _context.Products.Add(product);
              //  _context.SaveChanges();  // Save the product to the database
                int changes = _context.SaveChanges();
                Console.WriteLine($"{changes} records were updated.");
                // Redirect to the Index page to see the updated product list
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, reload the form with the existing categories
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }



        // Display Edit Product View
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // Handle Edit Product form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Update(product);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // Delete a Product
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}
