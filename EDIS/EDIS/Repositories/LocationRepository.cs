using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EDIS.Data;
using EDIS.Models.LocationModels;

namespace EDIS.Repositories
{
    public class BuildingRepository : IRepository<BuildingModel, int>
    {
        private readonly ApplicationDbContext _context;
        public BuildingRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public int Create(BuildingModel entity)
        {
            try
            {
                _context.Buildings.Add(entity);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }

            return entity.BuildingId;
        }

        public void Delete(int id)
        {
            _context.Remove(_context.Buildings.Find(id));
            _context.SaveChanges();
        }

        public IEnumerable<BuildingModel> Find(Expression<Func<BuildingModel, bool>> expression)
        {
            if (expression == null)
                return _context.Buildings.ToList();
            else
                return _context.Buildings.Where(expression);
        }

        public BuildingModel FindById(int id)
        {
            return _context.Buildings.Find(id);
        }

        public void Update(BuildingModel entity)
        {
            var oriBuilding = _context.Buildings.Single(x => x.BuildingId == entity.BuildingId);
            _context.Entry(oriBuilding).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }

    public class FloorRepository : IRepository<FloorModel, string[]>
    {
        private readonly ApplicationDbContext _context;
        public FloorRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public string[] Create(FloorModel entity)
        {
            try
            {
                _context.Floors.Add(entity);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }

            return new string[] { entity.BuildingId.ToString(), entity.FloorId};
        }

        public void Delete(string[] id)
        {
            int buildingid = Convert.ToInt32(id[0]);
            _context.Remove(_context.Floors.Single(x => x.BuildingId == buildingid && x.FloorId == id[1]));
            _context.SaveChanges();
        }

        public IEnumerable<FloorModel> Find(Expression<Func<FloorModel, bool>> expression)
        {
            if (expression == null)
                return _context.Floors.ToList();
            else
                return _context.Floors.Where(expression);
        }

        public FloorModel FindById(string[] id)
        {
            int buildingid = Convert.ToInt32(id[0]);
            return _context.Floors.SingleOrDefault(x => x.BuildingId == buildingid && x.FloorId == id[1]);
        }

        public void Update(FloorModel entity)
        {
            var oriFloor = _context.Floors.Single(x => x.BuildingId == entity.BuildingId &&
            x.FloorId == entity.FloorId);
            _context.Entry(oriFloor).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }

    public class PlaceRepository : IRepository<PlaceModel, string[]>
    {
        private readonly ApplicationDbContext _context;
        public PlaceRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public string[] Create(PlaceModel entity)
        {
            try
            {
                _context.Places.Add(entity);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }

            return new string[] { entity.BuildingId.ToString(), entity.FloorId, entity.PlaceId };
        }

        public void Delete(string[] id)
        {
            int buildingid = Convert.ToInt32(id[0]);
            _context.Remove(_context.Places.Single(x => x.BuildingId == buildingid && x.FloorId == id[1] 
            && x.PlaceId == id[2]));
            _context.SaveChanges();
        }

        public IEnumerable<PlaceModel> Find(Expression<Func<PlaceModel, bool>> expression)
        {
            if (expression == null)
                return _context.Places.ToList();
            else
                return _context.Places.Where(expression);
        }

        public PlaceModel FindById(string[] id)
        {
            int buildingid = Convert.ToInt32(id[0]);
            return _context.Places.SingleOrDefault(x => x.BuildingId == buildingid && x.FloorId == id[1] 
            && x.PlaceId == id[2]);
        }

        public void Update(PlaceModel entity)
        {
            var oriPlace = _context.Places.Single(x => x.BuildingId == entity.BuildingId &&
            x.FloorId == entity.FloorId && x.PlaceId == entity.PlaceId);
            _context.Entry(oriPlace).CurrentValues.SetValues(entity);
            _context.SaveChanges();
        }
    }
}
