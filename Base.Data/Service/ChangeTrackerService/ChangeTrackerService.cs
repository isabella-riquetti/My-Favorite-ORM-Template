using Base.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EntityState = System.Data.Entity.EntityState;

namespace Base.Data.Service.ChangeTrackerService
{
    public class ChangeTrackerService : IChangeTrackerService
    {
        private Guid GetPrimaryKeyValue(DbEntityEntry entry, IObjectContextAdapter productContext)
        {
            var objectStateEntry = productContext.ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.Entity).EntitySet;
            var keyName = objectStateEntry.ElementType.KeyMembers.Select(k => k.Name).ToArray().FirstOrDefault();

            var values = entry.State == EntityState.Deleted
                ? entry.OriginalValues.PropertyNames.ToDictionary(pn => pn, pn => entry.OriginalValues[pn])
                : entry.CurrentValues.PropertyNames.ToDictionary(pn => pn, pn => entry.CurrentValues[pn]);

            return keyName != null ? new Guid(values[keyName].ToString()) : new Guid();
        }

        public void SetCorrectOriginalValues(DbEntityEntry entry)
        {
            var values = entry.CurrentValues.Clone();
            var state = entry.State;
            entry.Reload();
            entry.CurrentValues.SetValues(values);
            entry.State = state;
        }

        private string GetEntityName(DbEntityEntry entry)
        {
            var baseType = entry.Entity.GetType().BaseType;
            var entityName = entry.State != EntityState.Added ? ((baseType != null) ? baseType.Name : null) : entry.Entity.GetType().Name;
            if (entityName == "Object")
                entityName = entry.Entity.GetType().Name;
            return entityName;
        }
    }
}
