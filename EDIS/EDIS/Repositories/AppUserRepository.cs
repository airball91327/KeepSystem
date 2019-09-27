using System;
using System.Collections.Generic;
using System.Linq;
using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models.RepairModels;

namespace EDIS.Repositories
{
    public class AppUserRepository : IRepository<AppUserModel, int>
    {
        private readonly ApplicationDbContext _context;

        public AppUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public int Create(AppUserModel entity)
        {
            _context.AppUsers.Add(entity);
            _context.SaveChanges();

            return entity.Id;
        }

        public void Delete(int id)
        {
            _context.Remove(_context.AppUsers.Single(x => x.Id == id));
            _context.SaveChanges();
        }

        public IEnumerable<AppUserModel> Find(System.Linq.Expressions.Expression<Func<AppUserModel, bool>> expression)
        {
            return _context.AppUsers.Where(expression);
        }

        public AppUserModel FindById(int id)
        {
            return _context.AppUsers.SingleOrDefault(x => x.Id == id);
        }

        public void Update(AppUserModel entity)
        {
            var oriUser = _context.AppUsers.Single(x => x.Id == entity.Id);
            _context.Entry(oriUser).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }
}
