using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace Base.Data.Programmability.Stored_Procedures
{
    public class StoredProcedures
    {
        private readonly DbContext _dbContext;
        public StoredProcedures(DbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
