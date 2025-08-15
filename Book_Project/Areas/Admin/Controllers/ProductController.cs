using Book_Project.DataAccess.Repository.IRepository;
using Book_Project.Models;
using Book_Project.Models.ViewModels;
using Book_Project.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Book_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _UnitOfWork = unitOfWork;
            _WebHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _UnitOfWork.Product.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var ProductInDb = _UnitOfWork.Product.Get(id);
            if (ProductInDb == null)
                return Json(new
                {
                    success = false,
                    message = "Something Went Wrong While Delete Data !!!"
                });
            //Image Delete
            var WebRootPath = _WebHostEnvironment.WebRootPath;
            var ImagePath = Path.Combine(WebRootPath, ProductInDb.ImageUrl.Trim('\\'));
            if (System.IO.File.Exists(ImagePath))
            {
                System.IO.File.Delete(ImagePath);
            }
            //*******
            _UnitOfWork.Product.Remove(ProductInDb);
            _UnitOfWork.Save();
            return Json(new
            {
                success = true,
                message = "Data Deleted Sucessfully !!!"
            });

        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product =new Product(),
                CategoryList =_UnitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                {
                    Text= cl.Name,
                    Value=cl.Id.ToString()
                }),
                CoverTypeList =_UnitOfWork.CoverType.GetAll().Select(ctl => new SelectListItem()
                {
                    Text= ctl.Name,
                    Value = ctl.Id.ToString()
                })
            };
            if (id == null) return View(productVM);
            productVM.Product =_UnitOfWork.Product.Get(id.GetValueOrDefault());
            if (productVM.Product == null) return NotFound();
            return View(productVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var WebRootpath = _WebHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count() > 0)
                {
                    var FileName = Guid.NewGuid().ToString();
                    var extention = Path.GetExtension(files[0].FileName);
                    var Upload = Path.Combine(WebRootpath, @"images\Products");     // @ is used bcz its is Escape Sequence ,(\)single is called as Escape Squence
                    if (productVM.Product.Id != 0)
                    {
                        var ImageExists = _UnitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = ImageExists;
                    }
                    if (productVM.Product.ImageUrl != null)
                    {
                        var ImagePath = Path.Combine(WebRootpath, productVM.Product.ImageUrl.Trim('\\'));
                        if (System.IO.File.Exists(ImagePath))
                        {
                            System.IO.File.Delete(ImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(Upload, FileName + extention)     // "Using" Is used for Disposable Object
                        , FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\Products\" + FileName + extention;
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var ImageExists = _UnitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = ImageExists;
                    }
                }
                if (productVM.Product.Id == 0)
                    _UnitOfWork.Product.Add(productVM.Product);
                else
                    _UnitOfWork.Product.Update(productVM.Product);
                _UnitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                productVM = new ProductVM()
                {
                    Product = new Product(),
                    CategoryList = _UnitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    }),
                    CoverTypeList = _UnitOfWork.CoverType.GetAll().Select(ctl => new SelectListItem()
                    {
                        Text = ctl.Name,
                        Value = ctl.Id.ToString()
                    })
                };
                if (productVM.Product.Id != 0)
                {
                    productVM.Product = _UnitOfWork.Product.Get(productVM.Product.Id);
                }
                return View(productVM);
            }
        }
    }
}
