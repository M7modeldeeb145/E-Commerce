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
    public class CompanyController : Controller
    {
        private IUnitOfWork unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var prods = unitOfWork.Company.GetAll().ToList();
            return View(prods);
        }
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                //create
                return View(new Company());
            }
            else
            {
                //update
                var Companyobj = unitOfWork.Company.Get(u => u.Id == id);
                return View(Companyobj);
            }

        }
        [HttpPost]
        public IActionResult Upsert(Company Company)
        {
            if (ModelState.IsValid)
            {
                if (Company.Id == 0)
                {
                    unitOfWork.Company.Create(Company);
                }
                else
                {
                    unitOfWork.Company.Update(Company);
                }
                unitOfWork.Save();
                TempData["success"] = "Company created/updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(Company);
            }
        }
        //public IActionResult Delete(int id)
        //{
        //    var Company = unitOfWork.Company.Get(e => e.Id == id);
        //    if (Company == null)
        //    {
        //        return NotFound();
        //    }
        //    unitOfWork.Company.Remove(Company);
        //    unitOfWork.Save();
        //    TempData["Success"] = "Company Successfully Deleted";
        //    return RedirectToAction("Index");
        //}
        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var Companies = unitOfWork.Company.GetAll().ToList();
            return Json(new { data = Companies });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companytobedeleted = unitOfWork.Company.Get(e => e.Id == id);
            if (companytobedeleted == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            unitOfWork.Company.Remove(companytobedeleted);
            unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }
        #endregion

    }
}

