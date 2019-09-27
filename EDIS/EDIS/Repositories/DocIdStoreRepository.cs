using System;
using System.Collections.Generic;
using System.Linq;
using EDIS.Data;
using EDIS.Models.Identity;
using EDIS.Models;

namespace EDIS.Repositories
{
    public class DocIdStoreRepository : IRepository<DocIdStore, string[]>
    {
        private readonly ApplicationDbContext _context;

        public DocIdStoreRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public string[] Create(DocIdStore entity)
        {
            _context.DocIdStores.Add(entity);
            _context.SaveChanges();

            return new string[] { entity.DocType, entity.DocId };
        }

        public void Delete(string[] id)
        {
            _context.Remove(_context.DocIdStores.Single(x => x.DocType == id[0] && x.DocId == id[1]));
            _context.SaveChanges();
        }

        public IEnumerable<DocIdStore> Find(System.Linq.Expressions.Expression<Func<DocIdStore, bool>> expression)
        {
            return _context.DocIdStores.Where(expression);
        }

        public DocIdStore FindById(string[] id)
        {
            return _context.DocIdStores.SingleOrDefault(x => x.DocType == id[0] && x.DocId == id[1]);
        }

        public void Update(DocIdStore entity)
        {
            var oriDpt = _context.DocIdStores.Single(x => x.DocType == entity.DocType);
            Create(entity);
            Delete(new string[] { oriDpt.DocType, oriDpt.DocId });
        }
    }
}
