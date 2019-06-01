[![Build Status](https://travis-ci.com/mrtryhard/DbAutoFillStandard.svg?branch=master)](https://travis-ci.com/mrtryhard/DbAutoFillStandard)

# DbAutoFillStandard
A .NETStandard version of the [Vircom's DbAutoFill](https://github.com/VircomOpenSource/DbAutoFill) library.

## Differences from original library
* Improved interface
* Multi-framework support through .NETStandard 2.0 
* Out-of-the-box DbCommandHelper support for SqlServer, Postgres (Npgsql), MySql databases
* Removal of SqlStructuredType as they were explicitely done for SQL Server. For now, IDbCustomType is support via full serialization of types.

## What is DbAutoFill ?
DbAutoFill allow you to map closely between your `IDataReader` and your objects. Technically speaking, it works for SQL readers but might as well work for any DataTable content.
This allows you to simplify your code from this:  
```
// Simplified code, without checks
IDataReader myDataReader = command.ExecuteReader();
myDataReader.Read();
MyObject obj = new MyObject();
obj.firstCol = myDataReader["firstCol"].ToString();
obj.secondCol = myDataReader["secondCol"].ToInt32();
obj.thirdCol = myDataReader["thirdCol"].ToString();
obj.fourthCol = myDataReader["fourthCol"].ToBool();
obj.fifthCol = myDataReader["fifthCol"].ToInt32();
// ...
```
To this:
```
IDataReader myDataReader = command.ExecuteReader();
myDataReader.Read();
MyObject obj = new MyObject();
DbAutoFillHelper.FillObjectFromDataReader(myDataReader, obj);
// ...
```

## Available operations
* Like the example above, you can automatically fill your object based on criterias offered through `DbAutoFillAttribute`.
* Additionnally, you may automatically fill your command parameters with the same principle with `FillDbParametersFromObject(command, obj)`
* Calling your stored procedures / functions and receiving the response close to automatically through `DbCommandHelper`.

## What's the difference with EntityFramework or X orm ?
* Besides the optional `DbCommandHelper`, it is not doing any SQL for you. Just filling parameters and/or objects for you.
* Just as easy to expand/refactor your codebase. 
* It is just as close as automated for you with the same security advantages.

## How does it does it ?
* In case of `DbAutoFillHelper`, most of the magic is done through reflection. However, the overhead should be very small.
* In case of `DbCommandHelper`, it's using `DbAutoFillHelper` underneath, and compiler services. It's not adding any overhead than what you would actually manually do.

## What is `DbAutoFillHelper`
It's the helper that add parameters to your sql command based on its member, fields and rules you set.
It's the same helper that fills your object from the `IDataReader` received from the database.

## What is `DbCommandHelper`
I found myself doing a lot of the same operation: create connection, create command, use `using`. 
Also, I always name my `C#` functions that call `SQL` procedures the same. Therefore, I've wrapped it 
with the help of the compiler services. Basically: your function name will be the function called on the sql server.
This offer a tight coupling between the SQL interface and your C# interface. 

## License
See LICENSE.md. Spoilers: it's MIT.

## Documentation / Examples
See [Vircom's DbAutoFill](https://github.com/VircomOpenSource/DbAutoFill), which contains some examples that are very close to how to use this version.
Some function, interface names are different as well as one or signature changes for uniformization.
