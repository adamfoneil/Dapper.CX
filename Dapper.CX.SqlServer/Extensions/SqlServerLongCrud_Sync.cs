using AO.DbSchema.Enums;
using System;
using System.Data;

namespace Dapper.CX.SqlServer.Extensions.Long
{
    public static partial class SqlServerLongCrud
    {
        public static TModel Get<TModel>(this IDbConnection connection, long id)
        {
            var provider = new SqlServerLongCrudProvider();
            return provider.Get<TModel>(connection, id);
        }

        public static TModel GetWhere<TModel>(this IDbConnection connection, object criteria)
        {
            var provider = new SqlServerLongCrudProvider();
            return provider.GetWhere<TModel>(connection, criteria);
        }

        public static void Delete<TModel>(this IDbConnection connection, int id)
        {
            var provider = new SqlServerLongCrudProvider();
            provider.Delete<TModel>(connection, id);
        }

        public static long Insert<TModel>(this IDbConnection connection, TModel model, Action<TModel, SaveAction> onSave = null, bool getIdentity = true)
        {
            var provider = new SqlServerLongCrudProvider();
            return provider.Insert(connection, model, onSave, getIdentity);
        }
    }
}
