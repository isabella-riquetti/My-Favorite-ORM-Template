using Base.Data.Models.Mapping;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;

namespace Base.Data.Context
{
    public partial class BaseContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<BaseContext>(null);
            modelBuilder.Configurations.Add(new UserMap());
        }
    }
}
