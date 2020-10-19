using Base.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Base.Data.Context;
using Base.Data.Bulk;
using System.Data.SqlClient;

namespace Base.Data.Repository.Base
{
    public class RepositoryBase<TEntity> : IDisposable, IRepositoryBase<TEntity> where TEntity : ModelBase
    {
        private readonly IBulkWorker<TEntity> _bulkWorker;
        internal IBaseContext Context;
        internal System.Data.Entity.DbSet<TEntity> DbSet;

        public RepositoryBase(IBaseContext context, IBulkWorker<TEntity> bulkWorker)
        {
            _bulkWorker = bulkWorker;
            this.Context = context;
            this.DbSet = context.Set<TEntity>();
        }

        public IBaseContext GetContext()
        {
            return Context;
        }

        /// <summary>
        /// Adiciona item no banco utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        /// <returns></returns>
        public TEntity Add(TEntity entity)
        {
            return DbSet.Add(entity);
        }

        /// <summary>
        /// Adiciona item no banco utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        /// <returns></returns>
        public T Add<T>(T entity) where T : ModelBase
        {
            //var _DbSet = Context.Set(typeof(T));
            var _DbSet = Context.Set<T>();

            return (T)_DbSet.Add(entity);
        }

        /// <summary>
        /// Adiciona itens utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        public void AddAll(List<TEntity> entity)
        {
            DbSet.AddRange(entity);
        }

        /// <summary>
        /// Adiciona itens utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        public void AddAll<T>(List<T> entity) where T : ModelBase
        {
            var _DbSet = Context.Set<T>();

            _DbSet.AddRange(entity);
        }

        /// <summary>
        /// Atualiza item utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser atualizada</param>
        public void Edit(TEntity entity)
        {
            Context.Entry<TEntity>(entity).State = System.Data.Entity.EntityState.Modified;
        }

        /// <summary>
        /// Atualiza item utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser atualizada</param>
        public void Edit<T>(T entity) where T : ModelBase
        {
            Context.Entry<TEntity>(entity as TEntity).State = System.Data.Entity.EntityState.Modified;
        }

        /// <summary>
        /// Atualiza itens utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser atualizada</param>
        public void EditAll(List<TEntity> entity)
        {
            foreach (var item in entity)
            {
                Context.Entry<TEntity>(item).State = System.Data.Entity.EntityState.Modified;
            }
        }

        /// <summary>
        /// Atualiza itens utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser atualizada</param>
        public void EditAll<T>(List<T> entity) where T : ModelBase
        {
            foreach (var item in entity)
            {
                Context.Entry<TEntity>(item as TEntity).State = System.Data.Entity.EntityState.Modified;
            }
        }

        /// <summary>
        /// Adiciona item utilizando SqlBulk. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entity">entidade a ser adicionada</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkAddWithTransaction<T>(T entity, SqlTransaction sqlTransaction)
        {
            var result = _bulkWorker.BulkAddWithTransaction(entity, sqlTransaction);
            return result;
        }

        /// <summary>
        /// Adiciona itens utilizando SqlBulk. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entities">entidades a serem adicionadas</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkAddRangeWithTransaction<T>(IEnumerable<T> entities, SqlTransaction sqlTransaction)
        {
            var result = _bulkWorker.BulkAddRangeWithTransaction(entities, sqlTransaction);
            return result;
        }

        /// <summary>
        /// Atualiza item utilizando SqlBulk. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entity">entidade a ser atualizada</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkEditWithTransaction<T>(T entity, SqlTransaction sqlTransaction)
        {
            var result = _bulkWorker.BulkEditWithTransaction(entity, sqlTransaction);
            return result;
        }

        /// <summary>
        /// Atualiza itens utilizando SqlBulk. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entities">entidades a serem atualizadas</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkEditRangeWithTransaction<T>(IEnumerable<T> entities, SqlTransaction sqlTransaction)
        {
            var result = _bulkWorker.BulkEditRangeWithTransaction(entities, sqlTransaction);
            return result;
        }

        /// <summary>
        /// Apaga itens utilizando SqlCommand. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entity">entidade a ser removida. Estes dados são utilizados apenas para log</param>
        /// <param name="idToRemove">Identificador do dado que será removido</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkDeleteWithTransaction<T>(T entity, Guid idToRemove, SqlTransaction sqlTransaction)
        {
            var result = _bulkWorker.BulkDeleteWithTransaction(entity, idToRemove, sqlTransaction);
            return result;
        }

        /// <summary>
        /// Apaga itens utilizando SqlCommand. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entities">entidades a serem removidas. Estes dados são utilizados apenas para log</param>
        /// <param name="idsToRemove">Identificadores dos dados que serão removidos</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkDeleteRangeWithTransaction<T>(IEnumerable<T> entities, List<Guid> idsToRemove, SqlTransaction sqlTransaction)
        {
            var result = _bulkWorker.BulkDeleteRangeWithTransaction(entities, idsToRemove, sqlTransaction);
            return result;
        }

        /// <summary>
        /// Adiciona item utilizando SqlBulk.
        /// </summary>
        /// <param name="entity">entidade a ser adicionada</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkAdd<T>(T entity, int timeOut = 20000, string connectionStringName = "BaseContext")
        {
            var result = _bulkWorker.BulkAdd(entity, timeOut, connectionStringName);
            return result;
        }

        /// <summary>
        /// Adiciona item utilizando SqlBulk (Não é necessário executar Commit).
        /// </summary>
        /// <param name="entities">entidades a serem adicionadas</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkAddRange<T>(IEnumerable<T> entities, int timeOut = 20000, string connectionStringName = "BaseContext")
        {
            var result = _bulkWorker.BulkAddRange(entities, timeOut, connectionStringName);
            return result;
        }

        /// <summary>
        /// Atualiza item utilizando SqlBulk (Não é necessário executar Commit).
        /// </summary>
        /// <param name="entity">entidade a ser adicionada</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkEdit<T>(T entity, int timeOut = 20000, string connectionStringName = "BaseContext")
        {
            var result = _bulkWorker.BulkEdit(entity, timeOut, connectionStringName);
            return result;
        }

        /// <summary>
        /// Adiciona itens utilizando SqlBulk (Não é necessário executar Commit).
        /// </summary>
        /// <param name="entities">entidades a serem atualizadas</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkEditRange<T>(IEnumerable<T> entities, int timeOut = 20000, string connectionStringName = "BaseContext")
        {
            var result = _bulkWorker.BulkEditRange(entities, timeOut, connectionStringName);
            return result;
        }

        /// <summary>
        /// Apaga itens utilizando SqlCommand (Não é necessário executar Commit).
        /// </summary>
        /// <param name="entities">entidades a serem removidas. Estes dados são utilizados apenas para log</param>
        /// <param name="idsToRemove">Identificadores dos dados que serão removidos</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkDeleteRange<T>(IEnumerable<T> entities, List<Guid> idsToRemove, int timeOut = 20000,
            string connectionStringName = "BaseContext")
        {
            var result = _bulkWorker.BulkDeleteRange(entities, idsToRemove, timeOut, connectionStringName);
            return result;
        }

        /// <summary>
        /// Apaga item utilizando SqlCommand (Não é necessário executar Commit).
        /// </summary>
        /// <param name="entity">entidade a ser removida. Estes dados são utilizados apenas para log</param>
        /// <param name="idToRemove">Identificador do dado que será removido</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        public OperationResult BulkDelete<T>(T entity, Guid idToRemove, int timeOut = 20000,
            string connectionStringName = "BaseContext")
        {
            var result = _bulkWorker.BulkDelete(entity, idToRemove, timeOut, connectionStringName);
            return result;
        }

        public void Delete(TEntity entity)
        {
            if (Context.Entry(entity).State == System.Data.Entity.EntityState.Detached)
            {
                DbSet.Attach(entity);
            }

            DbSet.Remove(entity);
        }

        public void Delete<T>(T entity) where T : ModelBase
        {
            var _DbSet = Context.Set(typeof(T));

            if (Context.Entry<TEntity>(entity as TEntity).State == System.Data.Entity.EntityState.Detached)
            {
                _DbSet.Attach(entity);
            }

            _DbSet.Remove(entity);
        }

        [Obsolete("This method is obsolete please use RemoveRange")]
        public void DeleteAll(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = DbSet;
            var listDelete = query.Where(filter).ToList();

            foreach (var item in listDelete)
            {
                DbSet.Remove(item);
            }
        }

        public virtual List<T> Get<T>(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includes) where T : ModelBase
        {
            using (new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew, new System.Transactions.TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                var _DbSet = Context.Set<T>();

                IQueryable<T> query = (System.Linq.IQueryable<T>)_DbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (includes != null)
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }

                if (orderBy != null)
                {
                    return orderBy(query).ToList();
                }
                else
                {
                    return query.ToList();
                }
            }
        }

        public virtual List<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            using (new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew, new System.Transactions.TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                IQueryable<TEntity> query = DbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (includes != null)
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }

                if (orderBy != null)
                {
                    return orderBy(query).ToList();
                }
                else
                {
                    return query.ToList();
                }
            }
        }

        public virtual IQueryable<TEntity> GetIQueryable(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, params Expression<Func<TEntity, object>>[] includes)
        {
            using (new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew, new System.Transactions.TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                IQueryable<TEntity> query = DbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (includes != null)
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }

                if (orderBy != null)
                {
                    return orderBy(query);
                }
                else
                {
                    return query;
                }
            }
        }

        public virtual IQueryable<T> GetIQueryable<T>(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includes) where T : ModelBase
        {
            using (new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew, new System.Transactions.TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {

                var _DbSet = Context.Set<T>();

                IQueryable<T> query = (System.Linq.IQueryable<T>)_DbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (includes != null)
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }

                if (orderBy != null)
                {
                    return orderBy(query);
                }
                else
                {
                    return query;
                }
            }
        }

        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
        {
            using (new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew, new System.Transactions.TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                IQueryable<TEntity> query = DbSet;

                if (filter != null)
                    query = query.Where(filter);

                return orderBy?.Invoke(query)?.FirstOrDefault() ?? query?.FirstOrDefault();
            }
        }

        public virtual T FirstOrDefault<T>(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null) where T : ModelBase
        {
            using (new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew, new System.Transactions.TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {

                var _DbSet = Context.Set(typeof(T));

                IQueryable<T> query = (System.Linq.IQueryable<T>)_DbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                if (orderBy != null)
                {
                    return orderBy(query).FirstOrDefault();
                }
                else
                {
                    return query.FirstOrDefault();
                }
            }
        }

        [Obsolete("Utilize o método FirstOrDefault")]
        public virtual TEntity SingleOrDefault(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
        {
            using (new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew, new System.Transactions.TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                IQueryable<TEntity> query = DbSet;

                if (filter != null)
                    query = query.Where(filter);

                return orderBy?.Invoke(query)?.SingleOrDefault();
            }
        }

        public void Dispose()
        {
            DbSet = null;
            Context.Dispose();
            GC.SuppressFinalize(this);
        }

        public void RemoveRange(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = DbSet;
            var listDelete = query.Where(filter).ToList();

            DbSet.RemoveRange(listDelete);
        }

        public void RemoveRange<T>(Expression<Func<T, bool>> filter = null)
        {
            var _DbSet = Context.Set(typeof(T));

            IQueryable<T> query = (System.Linq.IQueryable<T>)_DbSet;

            var listDelete = query.Where(filter).ToList();

            _DbSet.RemoveRange(listDelete);
        }
        ~RepositoryBase()
        {
            Dispose();
        }
    }
}
