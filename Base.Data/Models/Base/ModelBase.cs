using Base.Data.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Base.Data.Models
{
    public class ModelBase : INotifyPropertyChanged, IModelBase
    {
        public ModelBase(string tableName, string primaryKeyField)
        {          
            TableName = tableName;
            PrimaryKey = primaryKeyField;
        }        

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            var value = this.GetType().GetProperty(eventArgs.PropertyName).GetValue(this);
            if (value != null && !value.GetType().FullName.Contains("System.Collections"))
            {
                var primaryKey = PrimaryKey == eventArgs.PropertyName;

                RaisePropertyChanged(value != null ? value.ToString() : null, primaryKey, eventArgs.PropertyName);
            }
        }

        [NotMapped]        
        [IgnoreToDatatable(IgnorePropertyToDatatable = true)]
        public string TableName { get; set; }

        [NotMapped]        
        [IgnoreToDatatable(IgnorePropertyToDatatable = true)]
        public string PrimaryKey { get; set; }

        private Guid GetUserIdFromSession()
        {
            var currentHttpContext = HttpContext.Current;

            if (currentHttpContext?.Session == null || currentHttpContext.Session["UserId"] == null)
            {
                var id = GetUserIdFromClaimsIdentity(currentHttpContext);
                if (id != Guid.Empty)
                    return id;
                else
                    return new Guid("9C3B7854-0F07-4790-B54D-334705EE2985");
            }
            else
                return (Guid)currentHttpContext.Session["UserId"];
        }

        private Guid GetUserIdFromClaimsIdentity(HttpContext currentHttpContext)
        {
            if (currentHttpContext == null || currentHttpContext.User == null || currentHttpContext.User.Identity == null)
                return Guid.Empty;

            var claimsIdentity = currentHttpContext.User.Identity as System.Security.Claims.ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var userIdClaim = claimsIdentity.Claims
                    .FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                {
                    Guid userId;
                    Guid.TryParse(userIdClaim.Value, out userId);
                    return userId;
                }
            }

            return Guid.Empty;
        }

        public void RaisePropertyChanged(string newValue, bool primaryKey = false, [CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
