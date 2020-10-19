using Base.Data.Models;
using Base.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        Repository.Base.IRepositoryBase<ModelBase> RepositoryBase { get; set; }

        IStoredProcedureRepository StoredProcedure { get; set; }

        void Commit();
    }
}
