using Dapper.CX.Base.Enums;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Dapper.CX.Base.Classes
{
    public class CrudOptions<TModel>
    {
        public IEnumerable<Claim> Claims { get; set; }
        public ChangeTracker<TModel> ChangeTracker { get; set; }
        public Func<TModel, bool> AllowGet { get; set; }
        public Action<SaveAction, TModel> BeforeSave { get; set; }
        public Action<SaveAction, TModel> AfterSave { get; set; }        
    }
}
