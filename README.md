![logo](https://i.imgur.com/ERemSkU.png)
==============
**TimHanewich.Dataverse**: A lightweight library for interacting with Microsoft's Dataverse service (formerly Common Data Service or "XRM"), the native data service of Microsoft's Power Platform and Dynamics 365 platform.  
--------  
This library alleviates the need to create custom HTTP requests to interact with Dataverse. After providing the service with a few parameters, you can easily interact with Dataverse through the traditional CRUD operators.

## Install the package
To install the package in your .NET (C# or VB.NET) project, add the following package from NuGet:
[TimHanewich.Dataverse](https://www.nuget.org/packages/TimHanewich.Dataverse/)
Run this command in the .NET CLI:

    dotnet add package TimHanewich.Dataverse

### Authenticating with Dataverse
This class library supports authentication to the Dataverse web API. Use the `DataverseAuthenticator` class to authenticate.

    DataverseAuthenticator auth = new DataverseAuthenticator();
    auth.Username = "<your username>";
    auth.Password = "<your password>";
    auth.ClientId = Guid.Parse("51f81489-12ee-4a9e-aaae-a2591f45987d");
    auth.Resource = "https://<your org>.crm.dynamics.com/";
    await auth.GetAccessTokenAsync();

After running the `GetAccessTokenAsync` method, your access token will be stored inside the DataverseAuthenticator object as the `AccessToken` property. Provide this `AccessToken` to the `DataverseService` constructor (see below) to start using the Dataverse web API.

### Initializing the Dataverse Service
Place the following import statements at the top of your code file.

    using TimHanewich.Dataverse;  
    using Newtonsoft.Json;  
    using Newtonsoft.Json.Linq;

Create a new Dataverse Service

    DataverseService dv = new DataverseService("YourOrgName", "YourAccessToken");

In the above example, replace "YourOrgName" with the organization name for your environment. For example, if your environment URL is https://org109a2adc0.crm.dynamics.com/, your org name is "org109a2adc0".  

Replace "YourAccessToken" with the access token that you have received after authenticating through either the OAuth authentication flow or the above authentication using the `DataverseAuthenticator` class (recommended).

### Creating a record
We will use the `CreateAsync` method of the DataverseService to create a new record. This method accepts two parameters:  
- `setter` - the setter (schema) name of the entity. This can be found by going to https://<your_org_name>.crm.dynamics.com/api/data/v9.0/ in an authenticated window.  
- `json` - The JSON (`JObject`) content of the record that you are creating.  

A complete example of initializing a Dataverse Service and creating a new account record:

    DataverseService dv = new DataverseService("YourOrgName", "YourAccessToken");
    JObject jo = new JObject();
    jo.Add("name", "Microsoft"); //Specify name column of accounts table
    await dv.CreateAsync("accounts", jo).Wait();

### Reading, Updating, and Deleting
The reading, updating, and deleting methods are very similar to the `CreateAsync` method that is detailed above. The only difference is that these three methods also have a parameter called `id` which serves as the unique identifier of the record that you'd like to transact on.  
This `id` parameter is the GUID value associated with the record that you can find in Dynamics 365 or the Power Platform Dataverse portal.

## Working with Tables & Attributes
This package also provides the ability to read table and attribute structures. To leverage this capability import the resources by placing this statement at the top of your file:

    using TimHanewich.Dataverse.Metadata;

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
DataverseReadOperation read = new DataverseReadOperation();
read.TableIdentifier = "contacts";
read.RecordId = Guid.Parse("9b8b1f4d-da14-ec11-b6e6-000d3a99fcc1");
JArray QueryResults = await service.ReadAsync(read);
Console.WriteLine("Your one record:");
Console.WriteLine(QueryResults[0].ToString());
```
In the example above, the `service` object is an instance of the `DataverseService` class.

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
DataverseReadOperation read = new DataverseReadOperation();
read.TableIdentifier = "invoices";

DataverseReadFilter filter = new DataverseReadFilter();
filter.ColumnName = "total";
filter.Operator = ComparisonOperator.GreaterThan;
filter.SetValue(1000);
read.AddFilter(filter);

JArray results = await service.ExecuteDataverseReadOperationAsync(read);
```
If you need to use multiple filter statements, you can also do this:
```
DataverseReadOperation read = new DataverseReadOperation();
read.TableIdentifier = "invoices";

DataverseReadFilter filter = new DataverseReadFilter();
filter.ColumnName = "total";
filter.Operator = ComparisonOperator.GreaterThan;
filter.SetValue(1000);
read.AddFilter(filter);

DataverseReadFilter filter2 = new DataverseReadFilter();
filter2.LogicalOperatorPrefix = LogicalOperator.And;
filter2.ColumnName = "customer";
filter2.SetValue(Guid.Parse("9b8b1f4d-da14-ec11-b6e6-000d3a99fcc1"));
read.AddFilter(filter2);

JArray results = await service.ExecuteDataverseReadOperationAsync(read);
```
The key above is to define the `LogicalOperatorPrefix` property of the second filter. This is the logical prefix (for example "and", "or") that will be added between this filter and the preceeding filter.

### Request Data from a Related Record
Dataverse supports referrential table relationships via the **Lookup** data type. If you would like to include data from a related table, you can do so like this:
```
DataverseReadOperation read = new DataverseReadOperation();
read.TableIdentifier = "patients";

TableSelection related = new TableSelection();
related.TableIdentifier = "father";
read.Expand = related;
```
The key above is to set the `Expand` property of the `DataverseReadOperation` to a `TableSelection` instance.  
You can also limit to only certain columns you would like from the related table:
```
DataverseReadOperation read = new DataverseReadOperation();
read.TableIdentifier = "patients";

TableSelection related = new TableSelection();
related.TableIdentifier = "father";
related.AddColumn("firstname");
related.AddColumn("lastname");
related.AddColumn("dateofbirth");
read.Expand = related;
```

## Humanization
In 2023 and beyond, using private data with **large language models** like the "GPT" series by OpenAI is a commonly desired implementation across industries. As part of the *prompt engineering* process, developers must incorporate private data into prompts to these large language models. The data must be in a *understandable*, *legible*, and *human-readable* format for the LLM to understand and use in its response. I call this process *humanization*.

I've developed an algorithm to automatically perform this **humanization** process, **transforming the data from its raw form to a more human-readable form**. You can use the humanization capabilities of this package via the `HumanizeAsync` function of the `TimHanewich.Dataverse.Humanization` namespace:
```
DataverseService s = new DataverseService("<YOUR DATAVERSE INSTANCE URL>", "<DATAVERSE ACCESS TOKEN>");
JObject obj = await s.HumanizeAsync("cra0f_animal", Guid.Parse("79f11b28-30c7-ed11-b597-000d3a8c2011"));
Console.WriteLine(obj.ToString());
```
In the code above, you must provide the `HummanizeAsync` method two parameters:  
- The **logical name** of the table.
- The **unqiue identifier** (GUID) of the record you'd like to hummanize.

The code above will turn a **raw** Dataverse record, that looks like this:
```
{
  "@odata.context": "https://org3d1a4090.crm.dynamics.com/api/data/v9.0/$metadata#cra0f_animals/$entity",
  "@odata.etag": "W/\"4292472\"",
  "cra0f_favoritespecies": "238080000,238080001,238080002",
  "cra0f_dateofbirth": "2022-09-01T00:00:00Z",
  "_owningbusinessunit_value": "5bd9dba0-c95c-ed11-9562-000d3a1df4a2",
  "_cra0f_homepen_value": "95fc11dc-2fc7-ed11-b597-000d3a8c2011",
  "cra0f_species": 238080001,
  "statecode": 0,
  "statuscode": 1,
  "_cra0f_mother_value": "4f6a9a15-30c7-ed11-b597-000d3a8c2011",
  "_createdby_value": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "timezoneruleversionnumber": 4,
  "_ownerid_value": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "modifiedon": "2023-09-16T20:18:46Z",
  "_owninguser_value": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "_modifiedby_value": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "versionnumber": 4292472,
  "cra0f_name": "Phillip Piggy",
  "createdon": "2023-03-20T15:01:57Z",
  "cra0f_animalid": "79f11b28-30c7-ed11-b597-000d3a8c2011",
  "overriddencreatedon": null,
  "cra0f_wishesitwerea": null,
  "importsequencenumber": null,
  "_modifiedonbehalfby_value": null,
  "utcconversiontimezonecode": null,
  "_createdonbehalfby_value": null,
  "_owningteam_value": null
}
```
Into *this*, a much more human-readable form that can easily be understood and leverage by a large language model in answering questions. As you can see below, the "internal" names of the columns are converted to their display names, the values are translated into their *labels*, and arbitrary values that are rather specific to Dataverse are trimmed from the payload:
```
{
  "Favorite Species": "Cow; Pig; Chicken",
  "Date of Birth": "9/1/2022 12:00:00 AM",
  "Species": "Pig",
  "Modified On": "9/16/2023 8:18:46 PM",
  "Name": "Phillip Piggy",
  "Created On": "3/20/2023 3:01:57 PM"
}
```
The `HummanizeAsync` method also has a third (optional) parameter: `depth` (integer). This defines at what relational "depth" the hummanization should occur. For example, if one of the fields of the table you're hummanizing is a **lookup** field, specifing a depth > 0 will also hummanize the record that is being pointed to and **append this related record** to the returned payload. For example, at a depth of **1**, this would be the response instead:
```
{
  "Favorite Species": "Cow; Pig; Chicken",
  "Date of Birth": "9/1/2022 12:00:00 AM",
  "Owning Business Unit": {
    "Inheritance Mask": 1023,
    "Modified On": "11/5/2022 5:20:58 AM",
    "Created On": "11/5/2022 5:20:58 AM",
    "Is Disabled": false,
    "Name": "org3d1a4090"
  },
  "Home Pen": {
    "Modified On": "3/20/2023 2:59:49 PM",
    "Name": "Wayne Locks",
    "Created On": "3/20/2023 2:59:49 PM"
  },
  "Species": "Pig",
  "Mother": {
    "Date of Birth": "2/16/2023 12:00:00 AM",
    "Species": "Chicken",
    "Modified On": "3/20/2023 3:01:26 PM",
    "Name": "Caitlyn Chicky",
    "Created On": "3/20/2023 3:01:26 PM"
  },
  "Created By": {
    "Integration user mode": false,
    "Main Phone": "425-555-0100",
    "Access Mode": "Read-Write",
    "First Name": "System",
    "Restricted Access Mode": false,
    "Incoming Email Delivery Method": "Server-Side Synchronization or Email Router",
    "Primary Email": "admin@D365DemoTS909196.OnMicrosoft.com",
    "Unique user identity id": 3,
    "User Name": "admin@D365DemoTS909196.onmicrosoft.com",
    "Created On": "11/5/2022 5:21:02 AM",
    "Windows Live ID": "admin@D365DemoTS909196.onmicrosoft.com",
    "Full Name": "System Administrator",
    "License Type": "Enterprise",
    "Modified On": "11/10/2022 2:50:51 PM",
    "Azure State": "Exists",
    "Default Filters Populated": false,
    "Outgoing Email Delivery Method": "Server-Side Synchronization or Email Router",
    "Address": "US",
    "Primary Email Status": "Approved",
    "Last Name": "Administrator",
    "Mobile Phone": "425-555-0101",
    "User Synced": true,
    "User License Type": 59,
    "Deleted State": "Not deleted",
    "Default OneDrive for Business Folder Name": "CRM",
    "Email Address O365 Admin Approval Status": false,
    "Invitation Status": "Invitation Not Sent",
    "Yomi Full Name": "System Administrator",
    "Status": false,
    "User PUID": "100320013C21A393",
    "Country/Region": "US",
    "User Licensed": true
  },
  "Owner": {
    "Integration user mode": false,
    "Main Phone": "425-555-0100",
    "Access Mode": "Read-Write",
    "First Name": "System",
    "Restricted Access Mode": false,
    "Incoming Email Delivery Method": "Server-Side Synchronization or Email Router",
    "Primary Email": "admin@D365DemoTS909196.OnMicrosoft.com",
    "Unique user identity id": 3,
    "User Name": "admin@D365DemoTS909196.onmicrosoft.com",
    "Created On": "11/5/2022 5:21:02 AM",
    "Windows Live ID": "admin@D365DemoTS909196.onmicrosoft.com",
    "Full Name": "System Administrator",
    "License Type": "Enterprise",
    "Modified On": "11/10/2022 2:50:51 PM",
    "Azure State": "Exists",
    "Default Filters Populated": false,
    "Outgoing Email Delivery Method": "Server-Side Synchronization or Email Router",
    "Address": "US",
    "Primary Email Status": "Approved",
    "Last Name": "Administrator",
    "Mobile Phone": "425-555-0101",
    "User Synced": true,
    "User License Type": 59,
    "Deleted State": "Not deleted",
    "Default OneDrive for Business Folder Name": "CRM",
    "Email Address O365 Admin Approval Status": false,
    "Invitation Status": "Invitation Not Sent",
    "Yomi Full Name": "System Administrator",
    "Status": false,
    "User PUID": "100320013C21A393",
    "Country/Region": "US",
    "User Licensed": true
  },
  "Modified On": "9/16/2023 8:18:46 PM",
  "Owning User": {
    "Integration user mode": false,
    "Main Phone": "425-555-0100",
    "Access Mode": "Read-Write",
    "First Name": "System",
    "Restricted Access Mode": false,
    "Incoming Email Delivery Method": "Server-Side Synchronization or Email Router",
    "Primary Email": "admin@D365DemoTS909196.OnMicrosoft.com",
    "Unique user identity id": 3,
    "User Name": "admin@D365DemoTS909196.onmicrosoft.com",
    "Created On": "11/5/2022 5:21:02 AM",
    "Windows Live ID": "admin@D365DemoTS909196.onmicrosoft.com",
    "Full Name": "System Administrator",
    "License Type": "Enterprise",
    "Modified On": "11/10/2022 2:50:51 PM",
    "Azure State": "Exists",
    "Default Filters Populated": false,
    "Outgoing Email Delivery Method": "Server-Side Synchronization or Email Router",
    "Address": "US",
    "Primary Email Status": "Approved",
    "Last Name": "Administrator",
    "Mobile Phone": "425-555-0101",
    "User Synced": true,
    "User License Type": 59,
    "Deleted State": "Not deleted",
    "Default OneDrive for Business Folder Name": "CRM",
    "Email Address O365 Admin Approval Status": false,
    "Invitation Status": "Invitation Not Sent",
    "Yomi Full Name": "System Administrator",
    "Status": false,
    "User PUID": "100320013C21A393",
    "Country/Region": "US",
    "User Licensed": true
  },
  "Modified By": {
    "Integration user mode": false,
    "Main Phone": "425-555-0100",
    "Access Mode": "Read-Write",
    "First Name": "System",
    "Restricted Access Mode": false,
    "Incoming Email Delivery Method": "Server-Side Synchronization or Email Router",
    "Primary Email": "admin@D365DemoTS909196.OnMicrosoft.com",
    "Unique user identity id": 3,
    "User Name": "admin@D365DemoTS909196.onmicrosoft.com",
    "Created On": "11/5/2022 5:21:02 AM",
    "Windows Live ID": "admin@D365DemoTS909196.onmicrosoft.com",
    "Full Name": "System Administrator",
    "License Type": "Enterprise",
    "Modified On": "11/10/2022 2:50:51 PM",
    "Azure State": "Exists",
    "Default Filters Populated": false,
    "Outgoing Email Delivery Method": "Server-Side Synchronization or Email Router",
    "Address": "US",
    "Primary Email Status": "Approved",
    "Last Name": "Administrator",
    "Mobile Phone": "425-555-0101",
    "User Synced": true,
    "User License Type": 59,
    "Deleted State": "Not deleted",
    "Default OneDrive for Business Folder Name": "CRM",
    "Email Address O365 Admin Approval Status": false,
    "Invitation Status": "Invitation Not Sent",
    "Yomi Full Name": "System Administrator",
    "Status": false,
    "User PUID": "100320013C21A393",
    "Country/Region": "US",
    "User Licensed": true
  },
  "Name": "Phillip Piggy",
  "Created On": "3/20/2023 3:01:57 PM"
}
```

You can see in the code above, the **hummanized** version of several *related records* are also included in their appropriate properties. When extending to a depth of **2**, the related records of the core record's related records will also be included. As such, the `HummanizationAsync` method is **recursive**, using the product of itself to extend to related records.