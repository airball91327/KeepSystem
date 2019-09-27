using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EDIS.Data;
using EDIS.Models.RepairModels;

namespace EDIS.Repositories
{
    public class RepairRepository : IRepository<RepairModel, string>
    {
        private readonly ApplicationDbContext _context;

        public RepairRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public string Create(RepairModel entity)
        {
            try
            {
                _context.Repairs.Add(entity);
                _context.SaveChanges();
            }catch(Exception e)
            {
                throw e;
            }

            return entity.DocId;
        }

        public void Delete(string id)
        {
            _context.Remove(_context.Repairs.Single(x => x.DocId == id));
            _context.SaveChanges();
        }

        public IEnumerable<RepairModel> Find(System.Linq.Expressions.Expression<Func<RepairModel, bool>> expression)
        {
            if (expression == null)
                return _context.Repairs.ToList();
            else
                return _context.Repairs.Where(expression);
        }

        public RepairModel FindById(string id)
        {
            return _context.Repairs.SingleOrDefault(x => x.DocId == id);
        }

        public void Update(RepairModel entity)
        {
            var oriRepair = _context.Repairs.Single(x => x.DocId == entity.DocId);
            _context.Entry(oriRepair).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }

    public class RepairDtlRepository : IRepository<RepairDtlModel, string>
    {
        private readonly ApplicationDbContext _context;

        public RepairDtlRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public string Create(RepairDtlModel entity)
        {
            _context.RepairDtls.Add(entity);
            _context.SaveChanges();

            return entity.DocId;
        }

        public void Delete(string id)
        {
            _context.Remove(_context.RepairDtls.Single(x => x.DocId == id));
            _context.SaveChanges();
        }

        public IEnumerable<RepairDtlModel> Find(Expression<Func<RepairDtlModel, bool>> expression)
        {
            return _context.RepairDtls.Where(expression);
        }

        public RepairDtlModel FindById(string id)
        {
            return _context.RepairDtls.SingleOrDefault(x => x.DocId == id);
        }

        public void Update(RepairDtlModel entity)
        {
            var oriRepDtl = _context.RepairDtls.Single(x => x.DocId == entity.DocId);
            _context.Entry(oriRepDtl).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }

    public class RepairFlowRepository : IRepository<RepairFlowModel, string[]>
    {
        private readonly ApplicationDbContext _context;

        public RepairFlowRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public string[] Create(RepairFlowModel entity)
        {
            _context.RepairFlows.Add(entity);
            _context.SaveChanges();

            return new string[] {entity.DocId, entity.StepId.ToString()};
        }

        public void Delete(string[] id)
        {
            int stepid = Convert.ToInt32(id[1]);
            _context.Remove(_context.RepairFlows.Single(x => x.DocId == id[0] && x.StepId == stepid));
            _context.SaveChanges();
        }

        public IEnumerable<RepairFlowModel> Find(Expression<Func<RepairFlowModel, bool>> expression)
        {
            return _context.RepairFlows.Where(expression);
        }

        public RepairFlowModel FindById(string[] id)
        {
            int stepid = Convert.ToInt32(id[1]);
            return _context.RepairFlows.SingleOrDefault(x => x.DocId == id[0] && x.StepId == stepid);
        }

        public void Update(RepairFlowModel entity)
        {
            var oriRepFlow = _context.RepairFlows.Single(x => x.DocId == entity.DocId && 
                                                        x.StepId == entity.StepId);
            _context.Entry(oriRepFlow).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }

    public class RepairEmpRepository : IRepository<RepairEmpModel, string[]>
    {
        private readonly ApplicationDbContext _context;

        public RepairEmpRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public string[] Create(RepairEmpModel entity)
        {
            _context.RepairEmps.Add(entity);
            _context.SaveChanges();

            return new string[] { entity.DocId, entity.UserId.ToString() };
        }

        public void Delete(string[] id)
        {
            int userid = Convert.ToInt32(id[1]);
            _context.Remove(_context.RepairEmps.Single(x => x.DocId == id[0] && x.UserId == userid));
            _context.SaveChanges();
        }

        public IEnumerable<RepairEmpModel> Find(Expression<Func<RepairEmpModel, bool>> expression)
        {
            return _context.RepairEmps.Where(expression);
        }

        public RepairEmpModel FindById(string[] id)
        {
            int userid = Convert.ToInt32(id[1]);
            return _context.RepairEmps.SingleOrDefault(x => x.DocId == id[0] && x.UserId == userid);
        }

        public void Update(RepairEmpModel entity)
        {
            var oriRepEmp = _context.RepairEmps.Single(x => x.DocId == entity.DocId &&
                                                            x.UserId == entity.UserId);
            _context.Entry(oriRepEmp).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }
}
