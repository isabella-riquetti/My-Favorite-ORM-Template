using Base.Data.Context;
using Base.Data.Models;
using Base.Data.Repository._BulkBase;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace Base.Data.Repository.Bulk
{
    public class BulkBase<TEntity> : IDisposable, IBulkBase<TEntity> where TEntity : class
    {
        internal BaseContext Context;
        internal List<TEntity> EntitiesToInsert;
        internal List<TEntity> EntitiesToUpdate;
        internal SqlTransaction SqlTransaction;

        public BulkBase()
        {
            this.EntitiesToInsert = new List<TEntity>();
            OpenConnection();
        }

        private void OpenConnection()
        {
            var DB = new BaseDatabase("BaseContext", 20000);
            var connection = DB.Connection;
            connection.Open();

            var randomNumber = new Random();
            var transactionName = $"Transaction_{randomNumber.Next()}";

            SqlTransaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted, transactionName);
        }

        public void Add(TEntity entity)
        {
            EntitiesToInsert.Add(entity);
        }

        public void AddAll(List<TEntity> entities)
        {
            EntitiesToInsert.AddRange(entities);
        }

        public void Edit(TEntity entity)
        {
            EntitiesToUpdate.Add(entity);
        }
        
        public void EditAll(List<TEntity> entities)
        {
            EntitiesToUpdate.AddRange(entities);
        }

        public void Dispose()
        {
            EntitiesToInsert = null;
            EntitiesToUpdate = null;
            //Context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
