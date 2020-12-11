using AO.Models.Enums;
using AO.Models.Interfaces;
using Dapper.CX.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.CX.Interfaces
{
    public interface ISqlCrudProvider<TIdentity>
    {
        void Delete<TModel>(IDbConnection connection, TIdentity id, IDbTransaction txn = null);
        Task DeleteAsync<TModel>(IDbConnection connection, TIdentity id, IDbTransaction txn = null, IUserBase user = null);
        Task DeleteAsync<TModel>(IDbConnection connection, TModel model, IDbTransaction txn = null, IUserBase user = null);
        Task<bool> ExistsAsync<TModel>(IDbConnection connection, TIdentity id, IDbTransaction txn = null);
        Task<bool> ExistsWhereAsync<TModel>(IDbConnection connection, object criteria, IDbTransaction txn = null);
        TModel Get<TModel>(IDbConnection connection, TIdentity identity, IDbTransaction txn = null);
        Task<TModel> GetAsync<TModel>(IDbConnection connection, TIdentity identity, IDbTransaction txn = null, IUserBase user = null);
        string GetDeleteStatement(Type modelType);
        TIdentity GetIdentity<TModel>(TModel model);
        TModel GetWhere<TModel>(IDbConnection connection, object criteria, IDbTransaction txn = null);
        Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, object criteria, IDbTransaction txn = null, IUserBase user = null);
        TIdentity Insert<TModel>(IDbConnection connection, TModel model, bool getIdentity = true, IDbTransaction txn = null);
        Task<TIdentity> InsertAsync<TModel>(IDbConnection connection, TModel model, bool getIdentity = true, IDbTransaction txn = null, IUserBase user = null);
        bool IsNew<TModel>(TModel model);
        Task<TIdentity> MergeAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null, Action<SaveAction, TModel> onSave = null);
        Task<TIdentity> MergeAsync<TModel>(IDbConnection connection, TModel model, IEnumerable<string> keyProperties, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null, Action<SaveAction, TModel> onSave = null);
        TIdentity Save<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null);
        Task<TIdentity> SaveAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null, Action<SaveAction, TModel> onSave = null);
        Task<TIdentity> SaveAsync<TModel>(IDbConnection connection, TModel @object, params string[] columnNames);
        void Update<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null);
        void Update<TModel>(IDbConnection connection, TModel @object, IDbTransaction txn, params Expression<Func<TModel, object>>[] setColumns);
        void Update<TModel>(IDbConnection connection, TModel @object, params Expression<Func<TModel, object>>[] setColumns);
        Task UpdateAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null);
        Task UpdateAsync<TModel>(IDbConnection connection, TModel @object, IDbTransaction txn, params Expression<Func<TModel, object>>[] setColumns);
        Task UpdateAsync<TModel>(IDbConnection connection, TModel @object, params Expression<Func<TModel, object>>[] setColumns);
    }
}