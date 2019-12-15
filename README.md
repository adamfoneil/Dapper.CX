Nuget package **Dapper.CX** makes it easy to do CRUD operations on pure POCO model classes. The only model class requirement is that they have a property called `Id` or the class has an [Identity](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Attributes/IdentityAttribute.cs) attribute that indicates what its identity property is.

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
    
    await cn.SaveAsync(appt, ct); // with a change tracker object, only modified properties are included in update statement  
}
```
