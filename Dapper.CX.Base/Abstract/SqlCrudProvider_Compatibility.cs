﻿using AO.Models.Static;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract partial class SqlCrudProvider<TIdentity>
    {
        public async Task<TIdentity> SaveAsync<TModel>(IDbConnection connection, TModel @object, params string[] columnNames)
        {
            if (IsNew(@object))
            {
                var cmd = GetInsertStatement(typeof(TModel), columnNames);
                object id = await connection.ExecuteScalarAsync(cmd, @object);
                SetIdentity(@object, _convertIdentity(id));
            }
            else
            {
                var cmd = GetUpdateStatement<TModel>(columnNames: columnNames);
                await connection.ExecuteScalarAsync(cmd, @object);
            }

            return GetIdentity(@object);
        }

        public async Task UpdateAsync<TModel>(IDbConnection connection, TModel @object, IDbTransaction txn, params Expression<Func<TModel, object>>[] setColumns)
        {
            CommandDefinition cmd = GetSetColumnsUpdateCommand(@object, setColumns, txn);
            await connection.ExecuteAsync(cmd);
        }

        /// <summary>
        /// Performs a SQL update on select properties of an object
        /// </summary>        
        public async Task UpdateAsync<TModel>(IDbConnection connection, TModel @object, params Expression<Func<TModel, object>>[] setColumns)
        {
            CommandDefinition cmd = GetSetColumnsUpdateCommand(@object, setColumns);
            await connection.ExecuteAsync(cmd);
        }

        private CommandDefinition GetSetColumnsUpdateCommand<TModel>(TModel @object, Expression<Func<TModel, object>>[] setColumns, IDbTransaction txn = null)
        {
            var type = typeof(TModel);
            DynamicParameters dp = new DynamicParameters();

            string setColumnExpr = string.Join(", ", setColumns.Select(e =>
            {
                string propName = PropertyNameFromLambda(e);
                PropertyInfo pi = type.GetProperty(propName);
                dp.Add(propName, e.Compile().Invoke(@object));
                return $"{SqlBuilder.ApplyDelimiter(pi.GetColumnName(), StartDelimiter, EndDelimiter)}=@{propName}";
            }));

            string cmdText = $"UPDATE {SqlBuilder.ApplyDelimiter(type.GetTableName(), StartDelimiter, EndDelimiter)} SET {setColumnExpr} WHERE {SqlBuilder.ApplyDelimiter(type.GetIdentityName(), StartDelimiter, EndDelimiter)}=@id";
            dp.Add("id", GetIdentity(@object));

            Debug.Print(cmdText);

            return new CommandDefinition(cmdText, dp, transaction: txn);
        }

        private string PropertyNameFromLambda(Expression expression)
        {
            // thanks to http://odetocode.com/blogs/scott/archive/2012/11/26/why-all-the-lambdas.aspx
            // thanks to http://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression

            LambdaExpression le = expression as LambdaExpression;
            if (le == null) throw new ArgumentException("expression");

            MemberExpression me = null;
            if (le.Body.NodeType == ExpressionType.Convert)
            {
                me = ((UnaryExpression)le.Body).Operand as MemberExpression;
            }
            else if (le.Body.NodeType == ExpressionType.MemberAccess)
            {
                me = le.Body as MemberExpression;
            }

            if (me == null) throw new ArgumentException("expression");

            return me.Member.Name;
        }
    }
}
