using Base.Data.Models.Mapping;
using Base.Data.Programmability.Functions;
using Base.Data.Programmability.Stored_Procedures;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;

namespace Base.Data.Context
{
    public partial class BaseContext
    {
        public StoredProcedures StoredProcedures { get; set; }
        public ScalarValuedFunctions ScalarValuedFunctions { get; set; }
        public TableValuedFunctions TableValuedFunctions { get; set; }
    }
}
