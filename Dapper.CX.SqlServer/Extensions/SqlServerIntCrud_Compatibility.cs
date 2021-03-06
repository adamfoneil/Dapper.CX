﻿using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Extensions.Int
{
    public static partial class SqlServerIntCrud
    {
        public static async Task<int> SaveAsync<TModel>(this IDbConnection connection, TModel @object, params string[] columnNames)
        {
            return await GetProvider().SaveAsync(connection, @object, columnNames);
        }

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel @object, params Expression<Func<TModel, object>>[] setColumns)
        {
            await GetProvider().UpdateAsync(connection, @object, setColumns);
        }

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel @object, IDbTransaction txn, params Expression<Func<TModel, object>>[] setColumns)
        {
            await GetProvider().UpdateAsync(connection, @object, txn, setColumns);
        }
    }
}
