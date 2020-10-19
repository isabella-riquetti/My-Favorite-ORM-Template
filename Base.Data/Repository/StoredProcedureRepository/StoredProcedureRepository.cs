using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using Base.Data.Context;
using Base.Data.Models;

namespace Base.Data.Repository
{
    public class StoredProcedureRepository : IStoredProcedureRepository
    {
        private readonly IBaseContext _context;

        public StoredProcedureRepository(IBaseContext context) 
        {
            _context = context;
        }

        public bool AddMoovie()
        {
            return true;
        }

    }
}
