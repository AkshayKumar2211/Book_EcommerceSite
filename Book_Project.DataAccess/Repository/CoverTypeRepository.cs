using Book_Project.DataAccess.Data;
using Book_Project.DataAccess.Repository.IRepository;
using Book_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Project.DataAccess.Repository
{
    public class CoverTypeRepository:Repository<CoverType>,ICoverTypeRepository
    {
        private readonly ApplicationDbContext _context;
        public CoverTypeRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }
    }
}
