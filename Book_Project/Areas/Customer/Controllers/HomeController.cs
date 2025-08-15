using Book_Project.DataAccess.Data;
using Book_Project.DataAccess.Repository.IRepository;
using Book_Project.Models;
using Book_Project.Models.ViewModels;
using Book_Project.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Book_Project.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        

        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {

            var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            var Claims = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (Claims != null)
            {
                var Count = _unitOfWork.ShoppingCart.GetAll
                (sc => sc.ApplicationUserId == Claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
            }
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType"); // donot provide space between include properties it will cause an error
            return View(productList);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Details(int id) 
            {

            var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            var Claims = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (Claims != null)
            {
                var Count = _unitOfWork.ShoppingCart.GetAll
                (sc => sc.ApplicationUserId == Claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
            }
            var productInDb=_unitOfWork.Product.FirstOrDefault(p=>p.Id== id,IncludeProperties:"Category,CoverType");

            if (productInDb==null) return NotFound();

            var shoppingcart = new ShoppingCart()
            {
                Product=productInDb,
                ProductId=productInDb.Id
            };

            return View(shoppingcart);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            if (ModelState.IsValid)
            {
                var ClaimsIdentity = (ClaimsIdentity)(User.Identity);
                var Claims = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (Claims == null) return NotFound();
                shoppingCart.ApplicationUserId = Claims.Value;
                var shoppingCartInDb = _unitOfWork.ShoppingCart.FirstOrDefault
                (sc => sc.ApplicationUserId == Claims.Value && sc.ProductId == shoppingCart.ProductId);
                if (shoppingCartInDb == null)
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                else
                    shoppingCartInDb.Count += shoppingCart.Count;
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var ProductInDb = _unitOfWork.Product.FirstOrDefault(p => p.Id == shoppingCart.Id, IncludeProperties: "Category,CoverType");
                if (ProductInDb == null) return NotFound();
                var ShoppingCartEdit = new ShoppingCart()
                {
                    Product = ProductInDb,
                    ProductId = ProductInDb.Id
                };
                return View(ShoppingCartEdit);
            }
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
