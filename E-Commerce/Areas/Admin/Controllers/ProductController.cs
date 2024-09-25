using DeeboStore.DataAccess.Repository;
using DeeboStore.DataAccess.Repository.IRepository;
using DeeboStore.Models;
using DeeboStore.Models.ViewModels;
using DeeboStore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private IUnitOfWork unitOfWork;
        private IWebHostEnvironment webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }

        private IEnumerable<SelectListItem> GetCategoryList()
        {
            return unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
        }
        [HttpGet]
        public IActionResult Index()
        {
            var prods = unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(prods);
        }

        public IActionResult Upsert(int? id)
        {
            var productVM = new ProductVM
            {
                CategoryList = GetCategoryList(),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                // Create
                return View(productVM);
            }
            else
            {
                // Update
                productVM.Product = unitOfWork.Product.Get(u => u.Id == id);
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, "images/product");
                    Directory.CreateDirectory(productPath);
                    if (!string.IsNullOrEmpty(productVM.Product.ImgURL))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImgURL.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImgURL = "/images/product/" + fileName;
                }

                if (productVM.Product.Id == 0)
                {
                    unitOfWork.Product.Create(productVM.Product);
                }
                else
                {
                    unitOfWork.Product.Update(productVM.Product);
                }
                unitOfWork.Save();
                TempData["success"] = "Product created/updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                // Ensure CategoryList is populated in case of validation failure
                productVM.CategoryList = GetCategoryList();
                return View(productVM);
            }
        }

        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var prods = unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = prods });
        }
        [HttpDelete]
        public IActionResult Delete(int id) 
        {
            var producttobedeleted = unitOfWork.Product.Get(e=>e.Id == id);
            if (producttobedeleted == null)
            {
                return Json(new { success = false , message = "Error While Deleting" });
            }
            var oldimagepath = 
                Path.Combine(webHostEnvironment.WebRootPath, producttobedeleted.ImgURL.Trim('\\'));
            if (System.IO.File.Exists(oldimagepath))
            {
                System.IO.File.Delete(oldimagepath);
            }
            unitOfWork.Product.Remove(producttobedeleted); 
            unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }
        #endregion
    }
}
