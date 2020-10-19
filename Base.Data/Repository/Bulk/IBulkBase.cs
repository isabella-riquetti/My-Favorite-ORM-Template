using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Base.Data.Repository.Bulk
{
    public interface IBulkBase<TEntity> where TEntity : class
    {
        void Add(TEntity entity);

        void AddAll(List<TEntity> entity);

        void Edit(TEntity entity);

        void EditAll(List<TEntity> entity);
    }
}