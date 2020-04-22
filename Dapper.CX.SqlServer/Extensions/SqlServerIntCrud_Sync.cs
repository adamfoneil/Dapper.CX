using AO.DbSchema.Enums;
using Dapper.CX.Classes;
using System;
using System.Data;
using System.Linq.Expressions;

namespace Dapper.CX.SqlServer.Extensions.Int
{
    public static partial class SqlServerIntCrud
    {
        public static TModel Get<TModel>(this IDbConnection connection, int id, IDbTransaction txn = null)
        {
            var provider = new SqlServerIntCrudProvider();
            return provider.Get<TModel>(connection, id, txn);
        }

        public static TModel GetWhere<TModel>(this IDbConnection connection, object criteria, IDbTransaction txn = null)
        {
            var provider = new SqlServerIntCrudProvider();
            return provider.GetWhere<TModel>(connection, criteria, txn);
        }

        public static void Delete<TModel>(this IDbConnection connection, int id, IDbTransaction txn = null)
        {
            var provider = new SqlServerIntCrudProvider();
            provider.Delete<TModel>(connection, id, txn);
        }

        public static int Insert<TModel>(this IDbConnection connection, TModel model, Action<TModel, SaveAction> onSave = null, bool getIdentity = true, IDbTransaction txn = null)
        {
            var provider = new SqlServerIntCrudProvider();
            return provider.Insert(connection, model, onSave, getIdentity, txn);
        }

        public static void Update<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            var provider = new SqlServerIntCrudProvider();
            provider.Update(connection, model, changeTracker, onSave, txn);
        }

        public static void Update<TModel>(this IDbConnection connection, TModel model, params Expression<Func<TModel, object>>[] setColumns)
        {
            var provider = new SqlServerIntCrudProvider();
            provider.Update(connection, model, setColumns);
        }

        public static void Update<TModel>(this IDbConnection connection, TModel model, IDbTransaction txn, params Expression<Func<TModel, object>>[] setColumns)
        {
            var provider = new SqlServerIntCrudProvider();
            provider.Update(connection, model, txn, setColumns);
        }

        public static int Save<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            var provider = new SqlServerIntCrudProvider();
            return provider.Save(connection, model, changeTracker, onSave, txn);
        }
    }
}
