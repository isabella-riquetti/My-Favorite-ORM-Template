using Base.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Base.Data.Bulk
{
    public interface IBulkWorker<TEntity> where TEntity : ModelBase
    {
        OperationResult BulkAdd<T>(T entity, int timeOut, string connectionStringName);
        OperationResult BulkAddRange<T>(IEnumerable<T> entities, int timeOut, string connectionStringName);
        OperationResult BulkAddRangeWithTransaction<T>(IEnumerable<T> entities, SqlTransaction sqlTransaction);
        OperationResult BulkAddWithTransaction<T>(T entity, SqlTransaction sqlTransaction);
        OperationResult BulkDelete<T>(T entity, Guid idToRemove, int timeOut, string connectionStringName);
        OperationResult BulkDeleteRange<T>(IEnumerable<T> entities, List<Guid> idsToRemove, int timeOut, string connectionStringName);
        OperationResult BulkDeleteRangeWithTransaction<T>(IEnumerable<T> entities, List<Guid> idsToRemove, SqlTransaction sqlTransaction);
        OperationResult BulkDeleteWithTransaction<T>(T entity, Guid idToRemove, SqlTransaction sqlTransaction);
        void BulkEdit();
        OperationResult BulkEdit<T>(T entity, int timeOut, string connectionStringName);
        OperationResult BulkEditRange<T>(IEnumerable<T> entities, int timeOut, string connectionStringName);
        OperationResult BulkEditRangeWithTransaction<T>(IEnumerable<T> entities, SqlTransaction sqlTransaction);
        OperationResult BulkEditWithTransaction<T>(T entity, SqlTransaction sqlTransaction);
    }
}