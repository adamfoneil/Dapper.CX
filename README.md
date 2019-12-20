[![Build status](https://ci.appveyor.com/api/projects/status/90etxh1r0aycv1j9?svg=true)](https://ci.appveyor.com/project/adamosoftware/dapper-cx)

Nuget package **Dapper.CX.SqlServer** makes it easy to do CRUD operations on pure POCO model classes. The only model class requirement is that they have a property called `Id` or the class has an [Identity](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Attributes/IdentityAttribute.cs) attribute that indicates what its identity property is. [Why Dapper.CX?](https://github.com/adamosoftware/Dapper.CX/wiki)

Here's a simple example using [GetAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L41) and [SaveAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L51) methods assuming a fictional `Appointment` model class and fictional `GetConnection` method:
```csharp
using (var cn = GetConnection())
{
    var appt = await cn.GetAsync<Appointment>(id);
    
    // make your changes
    
    await cn.SaveAsync(appt);
}
```
The `SaveAsync` method performs an insert or update depending on whether the model class [IsNew](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L36) or not. Here's a more sophisticated example showing the [GetWhereAsync](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L46) method and [ChangeTracker](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Classes/ChangeTracker.cs) object.
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
