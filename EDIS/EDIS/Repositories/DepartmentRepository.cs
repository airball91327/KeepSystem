using System;
using System.Collections.Generic;
using System.Linq;
using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.RepairModels;

namespace EDIS.Repositories
{
    public class DepartmentRepository : IRepository<DepartmentModel, string>
    {
        private readonly ApplicationDbContext _context;

        public DepartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public string Create(DepartmentModel entity)
        {
            _context.Departments.Add(entity);
            _context.SaveChanges();

            return entity.DptId;
        }

        public void Delete(string id)
        {
            _context.Remove(_context.Departments.Single(x => x.DptId == id));
            _context.SaveChanges();
        }

        public IEnumerable<DepartmentModel> Find(System.Linq.Expressions.Expression<Func<DepartmentModel, bool>> expression)
        {
            return _context.Departments.Where(expression);
        }

        public DepartmentModel FindById(string id)
        {
            return _context.Departments.SingleOrDefault(x => x.DptId == id);
        }

        public void Update(DepartmentModel entity)
        {
            var oriDpt = _context.Departments.Single(x => x.DptId == entity.DptId);
            _context.Entry(oriDpt).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }
}
