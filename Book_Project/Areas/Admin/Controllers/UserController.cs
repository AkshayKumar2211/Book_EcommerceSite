using Book_Project.DataAccess.Data;
using Book_Project.DataAccess.Repository.IRepository;
using Book_Project.Models;
using Book_Project.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Book_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var userList=_context.ApplicationUsers.ToList();
            var roleList=_context.Roles.ToList();
            var userRoles=_context.UserRoles.ToList();

            foreach(var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(ur => ur.UserId==user.Id).RoleId;
                user.Role=roleList.FirstOrDefault(r => r.Id==roleId).Name;

                
                    if(user.CompanyId==null)
                    {
                        user.Company=new Company()
                        {
                            Name=""
                        };
                    }

                if (user.CompanyId != null)
                {
                    user.Company = new Company()
                    {
                        Name = _unitOfWork.Company.Get(Convert.ToInt32(user.CompanyId)).Name
                    };
                }
            }
            //Remove Admin RoleUser
            var AdminUser = userList.FirstOrDefault(u => u.Role==SD.Role_Admin);
            if (AdminUser != null) userList.Remove(AdminUser);
            return Json(new { data = userList });

        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            bool isLocked = false;
            var userInDb = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (userInDb == null)
                return Json(new { success = false, message = "Something wrong in Lock Unlock" });
            if (userInDb != null && userInDb.LockoutEnd > DateTime.Now)
            {
                userInDb.LockoutEnd = DateTime.Now;
                isLocked = false;
            }
            else
            {
                userInDb.LockoutEnd = DateTime.Now.AddYears(100);
                isLocked = true;
            }
            _context.SaveChanges();
            return Json(new { success = true, message = isLocked == true ? "User Successfully Locked" : "User Successfully UnLocked" });
        }



    }
}

