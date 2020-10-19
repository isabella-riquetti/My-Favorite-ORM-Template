using Base.Data.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Base.Data.Models
{
    public interface IModelBase
    {    

        event PropertyChangedEventHandler PropertyChanged;

        [NotMapped]
        [IgnoreToDatatable(IgnorePropertyToDatatable = true)]
        string TableName { get; set; }

        [NotMapped]
        [IgnoreToDatatable(IgnorePropertyToDatatable = true)]
        string PrimaryKey { get; set; }   

        void RaisePropertyChanged(string newValue, bool primaryKey = false, [CallerMemberName]string prop = "");
    }
}
