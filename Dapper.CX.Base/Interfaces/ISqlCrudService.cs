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
    public interface ISqlCrudService<TIdentity, TUser> where TUser : IUserBase
    {
        bool HasUser { get; }
        Func<TUser, Task> OnUserUpdatedAsync { get; set; }
        TUser User { get; }

        Task DeleteAsync<TModel>(IDbConnection connection, TIdentity id, Func<IDbConnection, IDbTransaction, Task> txnAction = null);
        Task DeleteAsync<TModel>(IDbConnection connection, TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null);
        Task DeleteAsync<TModel>(TIdentity id, Func<IDbConnection, IDbTransaction, Task> txnAction = null);
        Task DeleteAsync<TModel>(TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null);
        Task<bool> ExistsAsync<TModel>(IDbConnection connection, TIdentity id);
        Task<bool> ExistsAsync<TModel>(TIdentity id);
        Task<bool> ExistsWhereAsync<TModel>(IDbConnection connection, object criteria);
        Task<bool> ExistsWhereAsync<TModel>(object criteria);
        Task<TModel> GetAsync<TModel>(IDbConnection connection, TIdentity id);
        Task<TModel> GetAsync<TModel>(TIdentity id);
        IDbConnection GetConnection();
        Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, object criteria, IDbTransaction txn = null);
        Task<TModel> GetWhereAsync<TModel>(object criteria, IDbTransaction txn = null);
        Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, Expression<Func<TModel, bool>>[] criteria);
        Task<TModel> GetWhereAsync<TModel>(Expression<Func<TModel, bool>>[] criteria);
        Task<TIdentity> InsertAsync<TModel>(IDbConnection connection, TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null);
        Task<TIdentity> InsertAsync<TModel>(TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null);
        Task<TIdentity> MergeAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Func<IDbConnection, IDbTransaction, Task> txnAction = null, Action<SaveAction, TModel> onSave = null);
        Task<TIdentity> MergeAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Func<IDbConnection, IDbTransaction, Task> txnAction = null, Action<SaveAction, TModel> onSave = null);
        Task<TIdentity> SaveAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Func<IDbConnection, IDbTransaction, Task> txnAction = null, Action<SaveAction, TModel> onSave = null);
        Task<TIdentity> SaveAsync<TModel>(IDbConnection connection, TModel model, string[] columnNames);
        Task<TIdentity> SaveAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Func<IDbConnection, IDbTransaction, Task> txnAction = null, Action<SaveAction, TModel> onSave = null);
        Task<TIdentity> SaveAsync<TModel>(TModel model, string[] columnNames);
        Task<bool> TryDeleteAsync<TModel>(IDbConnection connection, TIdentity id, Func<Task> onSuccess = null, Action<Exception> onException = null);
        Task<bool> TryDeleteAsync<TModel>(IDbConnection connection, TModel model, Func<Task> onSuccess = null, Action<Exception> onException = null);
        Task<bool> TryDeleteAsync<TModel>(TIdentity id, Func<Task> onSuccess = null, Action<Exception> onException = null);
        Task<bool> TryDeleteAsync<TModel>(TModel model, Func<Task> onSuccess = null, Action<Exception> onException = null);
        Task<bool> TryInsertAsync<TModel>(IDbConnection connection, TModel model, Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null);
        Task<bool> TryInsertAsync<TModel>(TModel model, Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null);
        Task<bool> TryMergeAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null, Action<SaveAction, TModel> onSave = null);
        Task<bool> TryMergeAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null, Action<SaveAction, TModel> onSave = null);
        Task<TIdentity> TrySaveAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null, Action<SaveAction, TModel> onSave = null);
        Task<TIdentity> TrySaveAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null, Action<SaveAction, TModel> onSave = null);
        Task<bool> TryUpdateAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Func<Task> onSuccess = null, Action<Exception> onException = null);
        Task<bool> TryUpdateAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Func<Task> onSuccess = null, Action<Exception> onException = null);
        Task<bool> TryUpdateUserAsync(Func<Task> onSuccess = null, Action<Exception> onException = null);
        Task UpdateAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Func<IDbConnection, IDbTransaction, Task> txnAction = null);
        Task UpdateAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Func<IDbConnection, IDbTransaction, Task> txnAction = null);
        Task UpdateUserAsync(params Expression<Func<TUser, object>>[] setColumns);
        Task<IEnumerable<TResult>> QueryAsync<TResult>(object criteria = null);
    }
}