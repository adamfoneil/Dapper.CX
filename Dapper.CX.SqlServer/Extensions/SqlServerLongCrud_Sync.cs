using AO.DbSchema.Enums;
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
            var provider = new SqlServerLongCrudProvider();
            return provider.Get<TModel>(connection, id, txn);
        }

        public static TModel GetWhere<TModel>(this IDbConnection connection, object criteria, IDbTransaction txn = null)
        {
            var provider = new SqlServerLongCrudProvider();
            return provider.GetWhere<TModel>(connection, criteria, txn);
        }

        public static void Delete<TModel>(this IDbConnection connection, int id, IDbTransaction txn = null)
        {
            var provider = new SqlServerLongCrudProvider();
            provider.Delete<TModel>(connection, id, txn);
        }

        public static long Insert<TModel>(this IDbConnection connection, TModel model, Action<TModel, SaveAction> onSave = null, bool getIdentity = true, IDbTransaction txn = null)
        {
            var provider = new SqlServerLongCrudProvider();
            return provider.Insert(connection, model, onSave, getIdentity, txn);
        }

        public static void Update<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            var provider = new SqlServerIntCrudProvider();
            provider.Update(connection, model, changeTracker, onSave, txn);
        }

        public static void Update<TModel>(IDbConnection connection, TModel model, params Expression<Func<TModel, object>>[] setColumns)
        {
            var provider = new SqlServerIntCrudProvider();
            provider.Update(connection, model, setColumns);
        }

        public static long Save<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            var provider = new SqlServerIntCrudProvider();
            return provider.Save(connection, model, changeTracker, onSave, txn);
        }
    }
}
