[![Build status](https://ci.appveyor.com/api/projects/status/90etxh1r0aycv1j9?svg=true)](https://ci.appveyor.com/project/adamosoftware/dapper-cx) 
[![Nuget](https://img.shields.io/nuget/v/Dapper.CX.SqlServer?label=SqlServer)](https://www.nuget.org/packages/Dapper.CX.SqlServer/)
[![Nuget](https://img.shields.io/nuget/v/Dapper.CX.SqlServer.AspNetCore?label=AspNetCore)](https://www.nuget.org/packages/Dapper.CX.SqlServer.AspNetCore/)

Dapper.CX is a CRUD library for SQL Server made with Dapper. It works with POCO classes, where the only model class requirement is that they have a property called `Id` or an [Identity](https://github.com/adamosoftware/DbSchema.Attributes/blob/master/DbSchema.Attributes/Attributes/IdentityAttribute.cs) attribute on the class that indicates what its identity property is. `int` and `long` identity types are supported. You can use Dapper.CX in two ways:

- as an injected service, [learn more](https://github.com/adamfoneil/Dapper.CX/wiki/Using-Dapper.CX-with-Dependency-Injection). This is intended for .NET Core apps to use dependency injection along with user profile integration.
- as `IDbConnection` extension methods, [learn more](https://github.com/adamfoneil/Dapper.CX/wiki/Using-Dapper.CX-Extension-Methods). This is simpler to use than the service, but is not as elegant from a dependency standpoint.

Wiki links: [Why Dapper.CX?](https://github.com/adamosoftware/Dapper.CX/wiki), [Reference](https://github.com/adamosoftware/Dapper.CX/wiki/Crud-method-reference). Note that Dapper.CX doesn't create tables. Please see my [ModelSync](https://github.com/adamosoftware/ModelSync) project for info on that.

## In a Nutshell
When using the injected service, you'd write CRUD code that looks like this. This example assumes a fictional `Employee` model class. There are several advantages of using the injected service. One, it integrates nicely with the authenticated user to check permissions or perform audit and change tracking. Two, you can omit the `using` block that you otherwise need when interacting with a connection. Three, there are some handy overloads that bundle exception handling and more. Here's [how to implement the injected service](https://github.com/adamfoneil/Dapper.CX/wiki/Using-Dapper.CX-with-Dependency-Injection) along with a CRUD [method reference](https://github.com/adamfoneil/Dapper.CX/wiki/SqlCrudService-reference).

```csharp
public async Task<IActionResult> OnPostSaveAsync(Employee employee)
{
    await Data.SaveAsync(employee);
    return Redirect("/Employees");
}

public async Task<IActionResult> OnPostDeleteAsync(int id)
{
  await Data.DeleteAsync<Employee>(id);
  return Redirect("/Employees");
}
```

When using the extension methods, it's almost the same thing, but you must open a database connection first. This example assumes a fictional `GetConnection` method that opens a SQL Server connection.

```csharp
public async Task<IActionResult> OnPostSaveAsync(Employee employee)
{
   using (var cn = GetConnection())
   {
      await cn.SaveAsync(employee);
      return Redirect("/Employees");
   }
}

public async Task<IActionResult> OnPostDeleteAsync(int id)
{
  using (var cn = GetConnection())
  {
    await cn.DeleteAsync<Employee>(id);
    return Redirect("/Employees");
  }
}
```

## Customizing behaviors with interfaces
There's a lot of functionality you can opt into by implementing interfaces on your model classes from the [AO.Models](https://github.com/adamfoneil/Models) project. Available interfaces are [here](https://github.com/adamfoneil/Models/tree/master/Models/Interfaces).

## And one other thing...
In addition to the more common strong-typed CRUD operations, Dapper.CX also offers a [SqlCmdDictionary](https://github.com/adamosoftware/Dapper.CX/wiki/Using-SqlCmdDictionary) feature that gives you a clean way to build INSERT and UPDATE statements dynamically.

---
Please see also [Dapper.QX](https://github.com/adamosoftware/Dapper.QX), Dapper.CX's companion library.
