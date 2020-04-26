| 
[![Build status](https://ci.appveyor.com/api/projects/status/90etxh1r0aycv1j9?svg=true)](https://ci.appveyor.com/project/adamosoftware/dapper-cx) 
|
[![Nuget](https://img.shields.io/nuget/v/Dapper.CX.SqlServer?label=Nuget&20Crud)](https://www.nuget.org/packages/Dapper.CX.SqlServer/)
|
[![Nuget](https://img.shields.io/nuget/v/Dapper.CX.ChangeTracking?label=Nuget&20ChangeTracking)](https://www.nuget.org/packages/Dapper.CX.ChangeTracking/)

Nuget package **Dapper.CX.SqlServer** makes it easy to do CRUD operations on pure POCO model classes. The only model class requirement is that they have a property called `Id` or the class has an [Identity](https://github.com/adamosoftware/DbSchema.Attributes/blob/master/DbSchema.Attributes/Attributes/IdentityAttribute.cs) attribute that indicates what its identity property is.

Wiki links: [Why Dapper.CX?](https://github.com/adamosoftware/Dapper.CX/wiki), [Reference](https://github.com/adamosoftware/Dapper.CX/wiki/Crud-method-reference). Note that Dapper.CX doesn't create tables. Please see my [ModelSync](https://github.com/adamosoftware/ModelSync) project for info on that.

Here's a simple example using [GetAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L43) and [SaveAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L71) methods assuming a fictional `Appointment` model class and fictional `GetConnection` method:
```csharp
using (var cn = GetConnection())
{
    var appt = await cn.GetAsync<Appointment>(id);
    
    // make your changes
    
    await cn.SaveAsync(appt);
}
```
The `SaveAsync` method performs an insert or update depending on whether the model object [IsNew](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L38) or not. Here's a more sophisticated example showing the [GetWhereAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L52) method and [ChangeTracker](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Classes/ChangeTracker.cs) object. [Learn more](https://github.com/adamosoftware/Dapper.CX/wiki/Using-ChangeTracker) about tracking changes.
```csharp
using (var cn = GetConnection())
{
    var appt = await cn.GetWhereAsync<Appointment>(new { clientId = 4343, date = new DateTime(2020, 3, 1) });
    var ct = new ChangeTracker<Appointment>(appt);
    
    // make your changes
    
    // with a change tracker object, only modified properties are included in update statement 
    await cn.SaveAsync(appt, ct);  
}
```
If you need to update time and user audit tracking at the model level, you can use the `onSave` optional callback like this:
```csharp
using (var cn = GetConnection())
{
    var model = await cn.GetAsync<Employee>(id);
    
    // do some stuff with model
    
    await cn.SaveAsync(model, onSave: (row, action) =>
    {
        switch (action)
        {
            case SaveAction.Insert;                
                row.CreatedBy = User.Identity.Name;
                row.DateCreated = DateTime.UtcNow;
                break;
            case SaveAction.Update:
                row.ModifiedBy = User.Identity.Name;
                row.DateModified = DateTime.UtcNow;
                break;
        }
    });
}
```
In a real app, you'd likely extract the anonymous method to an actual method, and make it work as a convention across your application.

## Customizing behaviors with interfaces
There are some interfaces you can use on model classes to implement validation and custom `Get` behavior. To use these, your model class project must add package [AO.DbSchema.Attributes](https://github.com/adamosoftware/DbSchema.Attributes) as a dependency:

- [ICustomGet](https://github.com/adamosoftware/DbSchema.Attributes/blob/master/DbSchema.Attributes/Interfaces/ICustomGet.cs) lets you inject portions of the SQL statements that Dapper.CX generates. This is so you can add model properties that are populated from a custom query, but not part of your base table proper. See the [test](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/SqlServer/SqlServerIntegration.cs#L152) for an example, along with the related [model class](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/Models/EmployeeCustom.cs#L34..L39). Here's where `ICustomGet` is invoked [here](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L197) and [here](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L218). In that example, the properties `Balance` and `Whatever` aren't columns in the base table, but are queried during `Get` operations as if they are. (Note also that such properties use the `NotMapped` attribute to prevent Dapper.CX from attempting to bind them.)

- [IGetRelated](https://github.com/adamosoftware/DbSchema.Attributes/blob/master/DbSchema.Attributes/Interfaces/IGetRelated.cs) lets you implement navigation properties by injecting a delegate in which you can perform additional gets every time a model is queried. See the [test](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/SqlServer/SqlServerIntegration.cs#L161) for an example. This uses the same sample model class [above](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/Models/EmployeeCustom.cs#L41). Here's where `IGetRelated` is [invoked](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L61) internally.

- [IValidate](https://github.com/adamosoftware/DbSchema.Attributes/blob/master/DbSchema.Attributes/Interfaces/IValidate.cs) lets you perform validation on a model class prior to an insert or update. See [test](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/Validation.cs#L11) and related [model class](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/Models/EmployeeValid.cs) for example. Note that `IValidate` has two methods `Validate` and `ValidateAsync`. The async version passes a connection argument so you can perform that requires looking up something in the database. The sync version is for validating properties of the model that don't require any database lookup. Here's where `IValidate` is invoked [internally](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L184).

## And one other thing...
In addition to the more common strong-typed CRUD operations, Dapper.CX also offers a [SqlCmdDictionary](https://github.com/adamosoftware/Dapper.CX/wiki/Using-SqlCmdDictionary) feature that gives you a clean way to build INSERT and UPDATE statements dynamically.

---
Please see also [Dapper.QX](https://github.com/adamosoftware/Dapper.QX), Dapper.CX's companion library.
