TimHanewich.Cds
==============
A lightweight library for interacting with Microsoft's Dataverse service (formerly Common Data Service or "XRM"), the native data service of Microsoft's Power Platform and Dynamics 365 platform.  
--------  
This library alleviates the need to create custom HTTP requests to interact with Dataverse. After providing the service with a few parameters, you can easily interact with the CDS through the traditional CRUD operators.

## Install the package
To install the package in your .NET (C# or VB.NET) project, add the following package from NuGet:
[TimHanewich.Cds](https://www.nuget.org/packages/TimHanewich.Cds/)
Run this command in the .NET CLI:

    dotnet add package TimHanewich.Cds

### Authenticating with Dataverse
This class library supports authentication to the Dataverse web API. Use the `CdsAuthenticator` class to authenticate.

    CdsAuthenticator auth = new CdsAuthenticator();
    auth.Username = "<your username>";
    auth.Password = "<your password>";
    auth.ClientId = Guid.Parse("51f81489-12ee-4a9e-aaae-a2591f45987d");
    auth.Resource = "https://<your org>.crm.dynamics.com/";
    await auth.GetAccessTokenAsync();

After running the `GetAccessTokenAsync` method, your access token will be stored inside the CdsAuthenticator object as the `AccessToken` property. Provide this `AccessToken` to the `CdsService` constructor (see below) to start using the Dataverse web API.

### Initializing the CDS Service
Place the following import statements at the top of your code file.

    using TimHanewich.Cds;  
    using Newtonsoft.Json;  
    using Newtonsoft.Json.Linq;

Create a new CDS Service

    CdsService cds = new CdsService("YourOrgName", "YourAccessToken");

In the above example, replace "YourOrgName" with the organization name for your environment. For example, if your environment URL is https://org109a2adc0.crm.dynamics.com/, your org name is "org109a2adc0".  

Replace "YourAccessToken" with the access token that you have received after authenticating through either the OAuth authentication flow or the above authentication using the `CdsAuthenticator` class (recommended).

### Creating a record
We will use the `CreateRecordAsync` method of the CdsService to create a new record. This method accepts two parameters:  
`setter` - the setter (schema) name of the entity. This can be found by going to https://<your_org_name>.crm.dynamics.com/api/data/v9.0/ in an authenticated window.  
`object_json` - The JSON content of the record that you are creating.  

A complete example of initializing a CDS Service and creating a new account record:

    CdsService cds = new CdsService("YourOrgName", "YourAccessToken");
    await cds.CreateRecordAsync("accounts", "{\"name\":\"123 Industries\"}");

### Reading, Updating, and Deleting
The reading, updating, and deleting methods are very similar to the `CreateRecordAsync` method that is detailed above. The only difference is that these three methods also have a parameter called `id` which serves as the unique identifier of the record that you are trying to transact on.  
This `id` parameter is the GUID value associated with the record that you can find in Dynamics 365 or the Power Platform CDS portal.

## Working with Tables & Attributes
This package also provides the ability to read table and attribute structures. To leverage this capability import the resources by placing this statement at the top of your file:

    using TimHanewich.Cds.Metadata;

### Get a list of all records in the databse
Use the `GetEntityMetadataSummariesAsync` method to receive and array of record summaries:

    EntityMetadataSummary[] summaries = await service.GetEntityMetadataSummariesAsync();

Each `EntityMetadataSummary` object contains the name of the record and "URL Extension" of the record that can be used to query records of this entity type.

### Get metadata for a particular table
Use the `GetEntityMetadataAsync` method to access metadata for a particular table.

    EntityMetadata AccountsTableData = service.GetEntityMetadataAsync("account").Result;

The `EntityMetadata` object provides details about all attributes (columns) in the table:

    foreach (AttributeMetadata attribute in AccountsTableData.Attributes)
    {
        Console.WriteLine(attribute.DisplayName);
        Console.WriteLine(attribute.SchemaName);
        Console.WriteLine(attribute.LogicalName);
        Console.WriteLine(attribute.Description);
        Console.WriteLine(attribute.AttributeType.ToString());
    }

## Advanced Read Operation
This package also supports more complex read requests - for example, specifying certain columns to include, filtering based on column values, and requesting data from a related table.

### Request a Single Record
If you know the unique ID of the record you would like data for, you can request a single record as such:
```
CdsReadOperation read = new CdsReadOperation();
read.TableIdentifier = "contacts";
read.RecordId = Guid.Parse("9b8b1f4d-da14-ec11-b6e6-000d3a99fcc1");
JObject[] QueryResults = await service.ExecuteCdsReadOperationAsync(read);
Console.WriteLine("Your one record:");
Console.WriteLine(QueryResults[0].ToString());
```
In the example above, the `service` object is an instance of the `CdsService` class.

### Specify Certain Columns
To save bandwidth and improve download times, you can also specify certain columns to include. Building from the code snippet above:
```
read.AddColumn("firstname");
read.AddColumn("lastname");
```
*please note that if you do not specify a single column via the `AddColumn` method, all columns will be included by default.*

### Add a Query Filter (or multiple!)
You can also add a filter to your query. For example, if you are trying to select all invoices with a total value over $1,000:
```
CdsReadOperation read = new CdsReadOperation();
read.TableIdentifier = "invoices";

CdsReadFilter filter = new CdsReadFilter();
filter.ColumnName = "total";
filter.Operator = ComparisonOperator.GreaterThan;
filter.SetValue(1000);
read.AddFilter(filter);

JObject[] results = await service.ExecuteCdsReadOperationAsync(read);
```
If you need to use multiple filter statements, you can also do this:
```
CdsReadOperation read = new CdsReadOperation();
read.TableIdentifier = "invoices";

CdsReadFilter filter = new CdsReadFilter();
filter.ColumnName = "total";
filter.Operator = ComparisonOperator.GreaterThan;
filter.SetValue(1000);
read.AddFilter(filter);

CdsReadFilter filter2 = new CdsReadFilter();
filter2.LogicalOperatorPrefix = LogicalOperator.And;
filter2.ColumnName = "customer";
filter2.SetValue(Guid.Parse("9b8b1f4d-da14-ec11-b6e6-000d3a99fcc1"));
read.AddFilter(filter2);

JObject[] results = await service.ExecuteCdsReadOperationAsync(read);
```
The key above is to define the `LogicalOperatorPrefix` property of the second filter. This is the logical prefix (for example "and", "or") that will be added between this filter and the preceeding filter.

### Request Data from a Related Record
Dataverse supports referrential table relationships via the **Lookup** data type. If you would like to include data from a related table, you can do so like this:
```
CdsReadOperation read = new CdsReadOperation();
read.TableIdentifier = "patients";

TableSelection related = new TableSelection();
related.TableIdentifier = "father";
read.Expand = related;
```
The key above is to set the `Expand` property of the `CdsReadOperation` to a `TableSelection` instance.  
You can also limit to only certain columns you would like from the related table:
```
CdsReadOperation read = new CdsReadOperation();
read.TableIdentifier = "patients";

TableSelection related = new TableSelection();
related.TableIdentifier = "father";
related.AddColumn("firstname");
related.AddColumn("lastname");
related.AddColumn("dateofbirth");
read.Expand = related;
```

