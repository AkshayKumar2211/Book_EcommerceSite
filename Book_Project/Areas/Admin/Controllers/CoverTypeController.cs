using Book_Project.DataAccess.Repository.IRepository;
using Book_Project.Models;
using Book_Project.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Book_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;

        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var CoverList = _UnitOfWork.CoverType.GetAll();
            return Json(new { data = CoverList });

        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var CoverInDb = _UnitOfWork.CoverType.Get(id);
            if (CoverInDb == null)
                return Json(new { success = false, message = "Something Went Wrong While Delete Data!!!" });
            _UnitOfWork.CoverType.Remove(CoverInDb);
            _UnitOfWork.Save();
            return Json(new { Success = true, message = "Data Successfully Deleted" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            CoverType cover = new CoverType();
            if (id == null) return View(cover);    //CREATE

            //EDIT
            cover= _UnitOfWork.CoverType.Get(id.GetValueOrDefault());
            if (cover == null) return NotFound();
            return View(cover);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType cover)
        {
            if (cover == null) return NotFound();
            if (!ModelState.IsValid) return View(cover);
            if (cover.Id == 0)
                _UnitOfWork.CoverType.Add(cover);
            else
                _UnitOfWork.CoverType.Update(cover);
            _UnitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
