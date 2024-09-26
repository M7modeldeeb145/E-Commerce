using DeeboStore.DataAccess.Data;
using DeeboStore.DataAccess.Repository;
using DeeboStore.DataAccess.Repository.IRepository;
using DeeboStore.Models;
using DeeboStore.Models.ViewModels;
using DeeboStore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        { 
            return View();
        }
       
        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var Users = _context.ApplicationUsers.Include(u=>u.Company).ToList();
            foreach (var user in Users)
            {
                if (user.Company == null)
                {
                    user.Company = new() {Name="" };
                }
            }
            return Json(new { data = Users });
        }
        public IActionResult Delete(int id)
        {
            return Json(new { success = true, message = "Deleted Successfully" });
        }
        #endregion

    }
}

