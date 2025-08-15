using Book_Project.DataAccess.Repository.IRepository;
using Book_Project.Models;
using Book_Project.Models.ViewModels;
using Book_Project.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Book_Project.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork= unitOfWork;
        }
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)(User.Identity);
            var Claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (Claims == null)
            {
                ShoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = new List<ShoppingCart>()
                };
                return View(ShoppingCartVM);
            }

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == Claims
                .Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            //.....
            var Count = _unitOfWork.ShoppingCart.GetAll
           (sc => sc.ApplicationUserId == Claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
            // //......
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault
                (au => au.Id == Claims.Value);
            foreach (var List in ShoppingCartVM.ListCart)
            {
                List.Price = SD.GetPriceBasedOnQuantity
                    (List.Count, List.Product.Price, List.Product.Price50, List.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (List.Price*List.Count);
                if (List.Product.Description.Length > 100)
                {
                    List.Product.Description = List.Product.Description.Substring(0, 99) + "....";
                }
            }
            return View(ShoppingCartVM);
        }

        public IActionResult plus(int id)
        {
            var Cart = _unitOfWork.ShoppingCart.Get(id);
            Cart.Count += 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int id)
        {
            var Cart = _unitOfWork.ShoppingCart.Get(id);
            Cart.Count--;
            if (Cart.Count == 0)
            {
                var DEL = _unitOfWork.ShoppingCart.Get(id);
                _unitOfWork.ShoppingCart.Remove(DEL);
            }
            _unitOfWork.Save();
            //var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            //var Claims = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //if (Claims != null)
            //{
            //    var Count = _unitOfWork.ShoppingCart.GetAll
            //    (sc => sc.ApplicationUserId == Claims.Value).ToList().Count;
            //    HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
            //}
            return RedirectToAction(nameof(Index));
        }
        public IActionResult delete(int id)
        {
            var Cart = _unitOfWork.ShoppingCart.Get(id);
            _unitOfWork.ShoppingCart.Remove(Cart);
            _unitOfWork.Save();
            //var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            //var Claims = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //if (Claims != null)
            //{
            //    var Count = _unitOfWork.ShoppingCart.GetAll
            //    (sc => sc.ApplicationUserId == Claims.Value).ToList().Count;
            //    HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, Count);
            //}
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)(User.Identity);
            var Claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == Claims
                .Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault
               (au => au.Id == Claims.Value);
            foreach (var List in ShoppingCartVM.ListCart)
            {
                List.Price = SD.GetPriceBasedOnQuantity
                    (List.Count, List.Product.Price, List.Product.Price50, List.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (List.Price * List.Count);
                if (List.Product.Description.Length > 100)
                {
                    List.Product.Description = List.Product.Description.Substring(0, 99) + "....";
                }
            }
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var ClaimsIdentity = (ClaimsIdentity)(User.Identity);
            var Claims = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (Claims == null) return NotFound();
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == Claims.Value);
            ShoppingCartVM.ListCart =_unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == Claims.Value, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = Claims.Value;
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var List in ShoppingCartVM.ListCart)
            {
                List.Price = SD.GetPriceBasedOnQuantity
                (List.Count, List.Product.Price, List.Product.Price50, List.Product.Price100);
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId =List.ProductId,
                    OrderHeaderId =ShoppingCartVM.OrderHeader.Id,
                    Price =List.Price,
                    Count =List.Count,
                };
                ShoppingCartVM.OrderHeader.OrderTotal += (List.Count * List.Price);
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
            
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();

            //Session
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);
            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
