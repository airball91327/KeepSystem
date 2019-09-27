using System;
using System.Collections.Generic;
using System.Linq;
using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.RepairModels;

namespace EDIS.Repositories
{
    public class AppRoleRepository : IRepository<AppRoleModel, int>
    {
        private readonly ApplicationDbContext _context;

        public AppRoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public int Create(AppRoleModel entity)
        {
            _context.AppRoles.Add(entity);
            _context.SaveChanges();

            return entity.RoleId;
        }

        public void Delete(int id)
        {
            _context.Remove(_context.AppRoles.Single(x => x.RoleId == id));
            _context.SaveChanges();
        }

        public IEnumerable<AppRoleModel> Find(System.Linq.Expressions.Expression<Func<AppRoleModel, bool>> expression)
        {
            return _context.AppRoles.Where(expression);
        }

        public AppRoleModel FindById(int id)
        {
            return _context.AppRoles.SingleOrDefault(x => x.RoleId == id);
        }

        public void Update(AppRoleModel entity)
        {
            var oriRole = _context.AppRoles.Single(x => x.RoleId == entity.RoleId);
            _context.Entry(oriRole).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }
}
