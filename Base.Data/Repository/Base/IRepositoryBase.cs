using Base.Data.Context;
using Base.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Base.Data.Repository.Base
{
    public interface IRepositoryBase<TEntity> where TEntity : ModelBase
    {
        /// <summary>
        /// Retorna a instância do DbContext usada pelo RepositoryBase no momento.
        /// </summary>
        IBaseContext GetContext();

        /// <summary>
        /// Adiciona item no banco utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        /// <returns></returns>
        TEntity Add(TEntity entity);

        /// <summary>
        /// Adiciona item no banco utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        /// <returns></returns>
        T Add<T>(T entity) where T : ModelBase;

        /// <summary>
        /// Adiciona itens utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        void AddAll(List<TEntity> entity);

        /// <summary>
        /// Adiciona itens utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        void AddAll<T>(List<T> entity) where T : ModelBase;

        /// <summary>
        /// Atualiza item utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser atualizada</param>
        void Edit(TEntity entity);

        /// <summary>
        /// Atualiza item utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser atualizada</param>
        void Edit<T>(T entity) where T : ModelBase;

        /// <summary>
        /// /// Atualiza itens utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser atualizada</param>
        void EditAll(List<TEntity> entity);

        /// <summary>
        /// Atualiza itens utilizando o contexto do entity framework. Necessário utilizar o método Commit para confirmar a transação.
        /// </summary>
        /// <param name="entity">Entidade a ser atualizada</param>
        void EditAll<T>(List<T> entity) where T : ModelBase;

        /// <summary>
        /// Adiciona item utilizando SqlBulk. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entity">entidade a ser adicionada</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkAddWithTransaction<T>(T entity, SqlTransaction sqlTransaction);

        /// <summary>
        /// Adiciona itens utilizando SqlBulk. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entities">entidades a serem adicionadas</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkAddRangeWithTransaction<T>(IEnumerable<T> entities, SqlTransaction sqlTransaction);

        /// <summary>
        /// Atualiza item utilizando SqlBulk. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entity">entidade a ser atualizada</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkEditWithTransaction<T>(T entity, SqlTransaction sqlTransaction);

        /// <summary>
        /// Atualiza itens utilizando SqlBulk. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entities">entidades a serem atualizadas</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkEditRangeWithTransaction<T>(IEnumerable<T> entities, SqlTransaction sqlTransaction);

        /// <summary>
        /// Apaga itens utilizando SqlCommand. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entity">entidade a ser removida. Estes dados são utilizados apenas para log</param>
        /// <param name="idToRemove">Identificador do dado que será removido</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkDeleteWithTransaction<T>(T entity, Guid idToRemove, SqlTransaction sqlTransaction);

        /// <summary>
        /// Apaga itens utilizando SqlCommand. Este método deve ser chamado dentro de um escopo de transação.
        /// </summary>
        /// <param name="entities">entidades a serem removidas. Estes dados são utilizados apenas para log</param>
        /// <param name="idsToRemove">Identificadores dos dados que serão removidos</param>
        /// <param name="sqlTransaction">transação do sql</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkDeleteRangeWithTransaction<T>(IEnumerable<T> entities, List<Guid> idsToRemove, SqlTransaction sqlTransaction);

        /// <summary>
        /// Adiciona item utilizando SqlBulk.
        /// </summary>
        /// <param name="entity">entidade a ser adicionada</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkAdd<T>(T entity, int timeOut = 20000, string connectionStringName = "BaseContext");

        /// <summary>
        /// Adiciona item utilizando SqlBulk (Não é necessário executar Commit).
        /// </summary>
        /// <param name="entities">entidades a serem adicionadas</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkAddRange<T>(IEnumerable<T> entities, int timeOut = 20000, string connectionStringName = "BaseContext");

        /// <summary>
        /// Atualiza item utilizando SqlBulk (Não é necessário executar Commit).
        /// </summary>
        /// <param name="entity">entidade a ser adicionada</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkEdit<T>(T entity, int timeOut = 20000, string connectionStringName = "BaseContext");

        /// <summary>
        /// Adiciona itens utilizando SqlBulk (Não é necessário executar Commit).
        /// </summary>
        /// <param name="entities">entidades a serem atualizadas</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkEditRange<T>(IEnumerable<T> entities, int timeOut = 20000, string connectionStringName = "BaseContext");

        /// <summary>
        /// Apaga itens utilizando SqlCommand (Não é necessário executar Commit).
        /// </summary>
        /// <param name="entities">entidades a serem removidas. Estes dados são utilizados apenas para log</param>
        /// <param name="idsToRemove">Identificadores dos dados que serão removidos</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkDeleteRange<T>(IEnumerable<T> entities, List<Guid> idsToRemove, int timeOut = 20000,
            string connectionStringName = "BaseContext");

        /// <summary>
        /// Apaga item utilizando SqlCommand (Não é necessário executar Commit).
        /// </summary>
        /// <param name="entity">entidade a ser removida. Estes dados são utilizados apenas para log</param>
        /// <param name="idToRemove">Identificador do dado que será removido</param>
        /// <param name="timeOut">timeout do banco de dados</param>
        /// <param name="connectionStringName">nome da string de conexão</param>
        /// <param name="saveLog">Indica se deve ou não salvar log. Se true, considera dados da tabela SystemLogTables</param>
        /// <returns></returns>
        OperationResult BulkDelete<T>(T entity, Guid idToRemove, int timeOut = 20000, string connectionStringName = "BaseContext");

        void Delete(TEntity entity);

        void Delete<T>(T entity) where T : ModelBase;

        List<T> Get<T>(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includes) where T : ModelBase;

        List<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includes);

        IQueryable<TEntity> GetIQueryable(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includes);

        IQueryable<T> GetIQueryable<T>(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            params Expression<Func<T, object>>[] includes) where T : ModelBase;

        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null);

        T FirstOrDefault<T>(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null) where T : ModelBase;

        void RemoveRange(Expression<Func<TEntity, bool>> filter = null);

        void RemoveRange<T>(Expression<Func<T, bool>> filter = null);
    }
}
