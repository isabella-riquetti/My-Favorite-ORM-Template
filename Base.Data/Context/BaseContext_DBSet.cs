using Base.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Data.Context
{
    public partial class BaseContext
    {
        public DbSet<User> User { get; set; }
    }
}
