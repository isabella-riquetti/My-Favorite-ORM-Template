using Base.Data.Models;
using Base.Data.Repository.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Data.Repository
{
    public interface IStoredProcedureRepository
    {
        bool AddMoovie();
    }
}
