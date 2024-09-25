using DeeboStore.DataAccess.Repository.IRepository;
using DeeboStore.Models;
using DeeboStore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        IUnitOfWork unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var cats = unitOfWork.Category.GetAll().ToList();
            return View(cats);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order Cannot Match The Name");
            }
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Create(category);
                unitOfWork.Save();
                TempData["Success"] = "Category Successfully Created";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Edit(int id)
        {
            var cat = unitOfWork.Category.Get(e => e.Id == id);
            if (cat == null && cat.Id == 0 && cat.Id == null)
            {
                return NotFound();
            }
            return View(cat);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Update(category);
                unitOfWork.Save();
                TempData["Success"] = "Category Successfully Updated";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int id)
        {
            var cat = unitOfWork.Category.Get(e => e.Id == id);
            if (cat == null)
            {
                return NotFound();
            }
            unitOfWork.Category.Remove(cat);
            unitOfWork.Save();
            TempData["Success"] = "Category Successfully Deleted";
            return RedirectToAction("Index");
        }
    }
}
