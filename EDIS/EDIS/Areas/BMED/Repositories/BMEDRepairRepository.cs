using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EDIS.Areas.BMED.Data;
using EDIS.Areas.BMED.Models.RepairModels;

namespace EDIS.Areas.BMED.Repositories
{
    public class BMEDRepairRepository : BMEDIRepository<RepairModel, string>
    {
        private readonly BMEDDbContext _context;

        public BMEDRepairRepository(BMEDDbContext context)
        {
            _context = context;
        }

        public string Create(RepairModel entity)
        {
            try
            {
                _context.BMEDRepairs.Add(entity);
                _context.SaveChanges();
            }catch(Exception e)
            {
                throw e;
            }

            return entity.DocId;
        }

        public void Delete(string id)
        {
            _context.Remove(_context.BMEDRepairs.Single(x => x.DocId == id));
            _context.SaveChanges();
        }

        public IEnumerable<RepairModel> Find(System.Linq.Expressions.Expression<Func<RepairModel, bool>> expression)
        {
            if (expression == null)
                return _context.BMEDRepairs.ToList();
            else
                return _context.BMEDRepairs.Where(expression);
        }

        public RepairModel FindById(string id)
        {
            return _context.BMEDRepairs.SingleOrDefault(x => x.DocId == id);
        }

        public void Update(RepairModel entity)
        {
            var oriRepair = _context.BMEDRepairs.Single(x => x.DocId == entity.DocId);
            _context.Entry(oriRepair).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }

    public class BMEDRepairDtlRepository : BMEDIRepository<RepairDtlModel, string>
    {
        private readonly BMEDDbContext _context;

        public BMEDRepairDtlRepository(BMEDDbContext context)
        {
            _context = context;
        }
        public string Create(RepairDtlModel entity)
        {
            _context.BMEDRepairDtls.Add(entity);
            _context.SaveChanges();

            return entity.DocId;
        }

        public void Delete(string id)
        {
            _context.Remove(_context.BMEDRepairDtls.Single(x => x.DocId == id));
            _context.SaveChanges();
        }

        public IEnumerable<RepairDtlModel> Find(Expression<Func<RepairDtlModel, bool>> expression)
        {
            return _context.BMEDRepairDtls.Where(expression);
        }

        public RepairDtlModel FindById(string id)
        {
            return _context.BMEDRepairDtls.SingleOrDefault(x => x.DocId == id);
        }

        public void Update(RepairDtlModel entity)
        {
            var oriRepDtl = _context.BMEDRepairDtls.Single(x => x.DocId == entity.DocId);
            _context.Entry(oriRepDtl).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }

    public class BMEDRepairFlowRepository : BMEDIRepository<RepairFlowModel, string[]>
    {
        private readonly BMEDDbContext _context;

        public BMEDRepairFlowRepository(BMEDDbContext context)
        {
            _context = context;
        }
        public string[] Create(RepairFlowModel entity)
        {
            _context.BMEDRepairFlows.Add(entity);
            _context.SaveChanges();

            return new string[] {entity.DocId, entity.StepId.ToString()};
        }

        public void Delete(string[] id)
        {
            int stepid = Convert.ToInt32(id[1]);
            _context.Remove(_context.BMEDRepairFlows.Single(x => x.DocId == id[0] && x.StepId == stepid));
            _context.SaveChanges();
        }

        public IEnumerable<RepairFlowModel> Find(Expression<Func<RepairFlowModel, bool>> expression)
        {
            return _context.BMEDRepairFlows.Where(expression);
        }

        public RepairFlowModel FindById(string[] id)
        {
            int stepid = Convert.ToInt32(id[1]);
            return _context.BMEDRepairFlows.SingleOrDefault(x => x.DocId == id[0] && x.StepId == stepid);
        }

        public void Update(RepairFlowModel entity)
        {
            var oriRepFlow = _context.BMEDRepairFlows.Single(x => x.DocId == entity.DocId && 
                                                                  x.StepId == entity.StepId);
            _context.Entry(oriRepFlow).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }

    public class BMEDRepairEmpRepository : BMEDIRepository<RepairEmpModel, string[]>
    {
        private readonly BMEDDbContext _context;

        public BMEDRepairEmpRepository(BMEDDbContext context)
        {
            _context = context;
        }
        public string[] Create(RepairEmpModel entity)
        {
            _context.BMEDRepairEmps.Add(entity);
            _context.SaveChanges();

            return new string[] { entity.DocId, entity.UserId.ToString() };
        }

        public void Delete(string[] id)
        {
            int userid = Convert.ToInt32(id[1]);
            _context.Remove(_context.BMEDRepairEmps.Single(x => x.DocId == id[0] && x.UserId == userid));
            _context.SaveChanges();
        }

        public IEnumerable<RepairEmpModel> Find(Expression<Func<RepairEmpModel, bool>> expression)
        {
            return _context.BMEDRepairEmps.Where(expression);
        }

        public RepairEmpModel FindById(string[] id)
        {
            int userid = Convert.ToInt32(id[1]);
            return _context.BMEDRepairEmps.SingleOrDefault(x => x.DocId == id[0] && x.UserId == userid);
        }

        public void Update(RepairEmpModel entity)
        {
            var oriRepEmp = _context.BMEDRepairEmps.Single(x => x.DocId == entity.DocId &&
                                                                x.UserId == entity.UserId);
            _context.Entry(oriRepEmp).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }
}
