using Dapper.CX.Classes;
using System;
using System.Data;
using System.Linq.Expressions;

namespace Dapper.CX.SqlServer.Extensions.Long
{
    public static partial class SqlServerLongCrud
    {
        public static TModel Get<TModel>(this IDbConnection connection, long id, IDbTransaction txn = null)
        {            
            return GetProvider().Get<TModel>(connection, id, txn);
        }

        public static TModel GetWhere<TModel>(this IDbConnection connection, object criteria, IDbTransaction txn = null)
        {            
            return GetProvider().GetWhere<TModel>(connection, criteria, txn);
        }

        public static void Delete<TModel>(this IDbConnection connection, int id, IDbTransaction txn = null)
        {            
            GetProvider().Delete<TModel>(connection, id, txn);
        }

        public static long Insert<TModel>(this IDbConnection connection, TModel model, bool getIdentity = true, IDbTransaction txn = null)
        {            
            return GetProvider().Insert(connection, model, getIdentity, txn);
        }

        public static void Update<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null)
        {            
            GetProvider().Update(connection, model, changeTracker, txn);
        }

        public static void Update<TModel>(this IDbConnection connection, TModel model, params Expression<Func<TModel, object>>[] setColumns)
        {            
            GetProvider().Update(connection, model, setColumns);
        }

        public static void Update<TModel>(this IDbConnection connection, TModel model, IDbTransaction txn, params Expression<Func<TModel, object>>[] setColumns)
        {            
            GetProvider().Update(connection, model, txn, setColumns);
        }

        public static long Save<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null)
        {            
            return GetProvider().Save(connection, model, changeTracker, txn);
        }
    }
}
