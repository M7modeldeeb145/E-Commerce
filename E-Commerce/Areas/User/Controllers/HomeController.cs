using DeeboStore.DataAccess.Repository.IRepository;
using DeeboStore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace E_Commerce.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products= _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return View(products);
        }
        public IActionResult Details(int id)
        {
            var cart = new ShoppingCart()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category"),
                Count = 1,
                ProductId = id
            };
            return View(cart);
        }
        [HttpPost]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            var cartfromdb = _unitOfWork.ShoppingCart.Get(u=>u.ApplicationUserId == userId&&
            u.ProductId == shoppingCart.ProductId);
            if (cartfromdb != null)
            {
                cartfromdb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartfromdb);
            }
            else
            {
                shoppingCart.Id = 0;
                _unitOfWork.ShoppingCart.Create(shoppingCart);
            }
            TempData["success"] = "Cart Updated Successfully";

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
