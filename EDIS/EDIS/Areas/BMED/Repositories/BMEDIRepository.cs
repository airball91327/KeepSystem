using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EDIS.Areas.BMED.Repositories
{
    public interface BMEDIRepository<TEntity, TKey>
        where TEntity : class
    {
        TKey Create(TEntity entity);
        void Update(TEntity entity);
        void Delete(TKey id);
        TEntity FindById(TKey id);
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> expression );
    }
}
