using Dapper.CX.Base.Enums;
using System;

namespace Dapper.CX.Base.Classes
{
    public class CrudOptions<TModel>
    {
        public Action<SaveAction, TModel> BeforeSave { get; set; }
        public Action<SaveAction, TModel> AfterSave { get; set; }        
    }
}
