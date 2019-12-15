Nuget package **Dapper.CX.SqlServer** makes it easy to do CRUD operations on pure POCO model classes. The only model class requirement is that they have a property called `Id` or the class has an [Identity](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Attributes/IdentityAttribute.cs) attribute that indicates what its identity property is.

Here's a simple example using [GetAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L41) and [SaveAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L51) methods assuming a fictional `Appointment` model class and fictional `GetConnection` method:
```
using (var cn = GetConnection())
{
    var appt = await cn.GetAsync<Appointment>(id);
    
    // make your changes
    
    await cn.SaveAsync(appt);
}
```
The `SaveAsync` method performs an insert or update depending on whether the model class [IsNew](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L36) or not. Here's a more sophisticated example showing the [GetWhereAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L46) method and [ChangeTracker](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Classes/ChangeTracker.cs) object.
```
using (var cn = GetConnection())
{
    var appt = await cn.GetWhereAsync<Appointment>(new { clientId = 4343, date = new DateTime(2020, 3, 1) });
    var ct = new ChangeTracker<Appointment>(appt);
    
    // make your changes
    
    // with a change tracker object, only modified properties are included in update statement 
    await cn.SaveAsync(appt, ct);  
}
```
## Reference
- Task [DeleteAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L127)&lt;TIdentity&gt;
  (IDbConnection connection, TIdentity id)
- Task&lt;TModel&gt; [GetAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L41)&lt;TIdentity&gt;
  (IDbConnection connection, TIdentity identity)
- string [GetDeleteStatement](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L203)
  (Type modelType)
- TIdentity [GetIdentity](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L29)&lt;TModel&gt;
  (TModel model)
- string [GetInsertStatement](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L158)
  (Type modelType)
- string [GetQuerySingleStatement](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L142)
  (Type modelType)
- string [GetQuerySingleWhereStatement](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L147)
  (Type modelType, object criteria)
- string [GetQuerySingleWhereStatement](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L153)
  (Type modelType, IEnumerable&lt;PropertyInfo&gt; properties)
- string [GetUpdateStatement](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L170)&lt;TModel&gt;
  (TModel model, [ChangeTracker&lt;TModel&gt;](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Classes/ChangeTracker.cs#L10) changeTracker)
- Task&lt;TModel&gt; [GetWhereAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L46)
  (IDbConnection connection, object criteria)
- Task&lt;TIdentity&gt; [InsertAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L97)&lt;TModel&gt;
  (IDbConnection connection, TModel model, Action&lt;TModel, SaveAction&gt; onSave)
- bool [IsNew](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L36)&lt;TModel&gt;
  (TModel model)
- Task&lt;TIdentity&gt; [MergeAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L64)&lt;TModel&gt;
  (IDbConnection connection, TModel model, [ChangeTracker&lt;TModel&gt;](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Classes/ChangeTracker.cs#L10) changeTracker, Action&lt;TModel, SaveAction&gt; onSave)
- Task&lt;TIdentity&gt; [SaveAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L51)&lt;TModel&gt;
  (IDbConnection connection, TModel model, [ChangeTracker&lt;TModel&gt;](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Classes/ChangeTracker.cs#L10) changeTracker, Action&lt;TModel, SaveAction&gt; onSave)
- Task [UpdateAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L112)&lt;TModel&gt;
  (IDbConnection connection, TModel model, [ChangeTracker&lt;TModel&gt;](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Classes/ChangeTracker.cs#L10) changeTracker, Action&lt;TModel, SaveAction&gt; onSave)
