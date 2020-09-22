[![Build status](https://ci.appveyor.com/api/projects/status/90etxh1r0aycv1j9?svg=true)](https://ci.appveyor.com/project/adamosoftware/dapper-cx) 
[![Nuget](https://img.shields.io/nuget/v/Dapper.CX.SqlServer?label=SqlServer)](https://www.nuget.org/packages/Dapper.CX.SqlServer/)
[![Nuget](https://img.shields.io/nuget/v/Dapper.CX.SqlServer.AspNetCore?label=AspNetCore)](https://www.nuget.org/packages/Dapper.CX.SqlServer.AspNetCore/)

Dapper.CX is a CRUD extension library for SQL Server made with Dapper. The only model class requirement is that they have a property called `Id` or the class has an [Identity](https://github.com/adamosoftware/DbSchema.Attributes/blob/master/DbSchema.Attributes/Attributes/IdentityAttribute.cs) attribute that indicates what its identity property is. `int` and `long` identity types are supported. You can use Dapper.CX in two ways:

- as an injected service, [learn more](https://github.com/adamfoneil/Dapper.CX/wiki/Using-Dapper.CX-with-Dependency-Injection). This is intended for .NET Core apps to use dependency injection along with tight user profile integration.
- as `IDbConnection` extension methods, [learn more](https://github.com/adamfoneil/Dapper.CX/wiki/Using-Dapper.CX-Extension-Methods). This is simpler to use than the service, but is not as elegant from a dependency standpoint.

Wiki links: [Why Dapper.CX?](https://github.com/adamosoftware/Dapper.CX/wiki), [Reference](https://github.com/adamosoftware/Dapper.CX/wiki/Crud-method-reference). Note that Dapper.CX doesn't create tables. Please see my [ModelSync](https://github.com/adamosoftware/ModelSync) project for info on that.

## Customizing behaviors with interfaces
There are some interfaces you can use on model classes to implement validation and custom `Get` behavior. To use these, your model class project must add package [AO.Models](https://github.com/adamfoneil/Models) as a dependency. Here's a sample of things you can implement:

- [IPermission](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IPermission.cs) lets you check whether the current user has permission to save, delete, or get a row from the database. This is for role-based permission checks.

- [IAudit](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IAudit.cs) lets you standardize how user and timestamps are saved throughout your application. See also [IUserBase](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/ITenantUser.cs#L5).

- [ITenantIsolated](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/ITenantIsolated.cs) lets you specify a tenant Id value on a model instance that can be compared with an [ITenantUser](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/ITenantUser.cs#L15) `TenantId` to see if they have permission to that record.

- [ITrigger](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/ITrigger.cs) works much like database triggers, letting you specify events to happen after a row is saved or deleted.

- [ICustomGet](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/ICustomGet.cs) lets you inject portions of the SQL statements that Dapper.CX generates. This is so you can add model properties that are populated from a custom query, but not part of your base table. See the [test](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/SqlServer/SqlServerIntegration.cs#L152) for an example, along with the related [model class](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/Models/EmployeeCustom.cs#L34..L39). Here's where `ICustomGet` is invoked [here](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L197) and [here](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L218). In that example, the properties `Balance` and `Whatever` aren't columns in the base table, but are queried during `Get` operations as if they are. (Note also that such properties use the `NotMapped` attribute to prevent Dapper.CX from attempting to bind them.)

- [IGetRelated](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IGetRelated.cs) lets you implement navigation properties by injecting a delegate in which you can perform additional gets every time a model is queried. See the [test](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/SqlServer/SqlServerIntegration.cs#L161) for an example. This uses the same sample model class [above](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/Models/EmployeeCustom.cs#L41). Here's where `IGetRelated` is [invoked](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L61) internally.

- [IValidate](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IValidate.cs) lets you perform validation on a model class prior to an insert or update. See [test](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/Validation.cs#L11) and related [model class](https://github.com/adamosoftware/Dapper.CX/blob/master/Tests.SqlServer/Models/EmployeeValid.cs) for example. Note that `IValidate` has two methods `Validate` and `ValidateAsync`. The async version passes a connection argument so you can perform that requires looking up something in the database. The sync version is for validating properties of the model that don't require any database lookup. Here's where `IValidate` is invoked [internally](https://github.com/adamosoftware/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs#L184).

## And one other thing...
In addition to the more common strong-typed CRUD operations, Dapper.CX also offers a [SqlCmdDictionary](https://github.com/adamosoftware/Dapper.CX/wiki/Using-SqlCmdDictionary) feature that gives you a clean way to build INSERT and UPDATE statements dynamically.

---
Please see also [Dapper.QX](https://github.com/adamosoftware/Dapper.QX), Dapper.CX's companion library.
