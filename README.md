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