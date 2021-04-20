TimHanewich.Cds
==============
A lightweight library for interacting with Microsoft's Dataverse service (formerly Common Data Service or "XRM"), the native data service of Microsoft's Power Platform and Dynamics 365 platform.  
--------  
This library alleviates the need to create custom HTTP requests to interact with Dataverse. After providing the service with a few parameters, you can easily interact with the CDS through the traditional CRUD operators.

### Initializing the CDS Service
Place the following import statements at the top of your code file.

    using TimHanewich.Cds;  
    using Newtonsoft.Json;  
    using Newtonsoft.Json.Linq;

Create a new CDS Service

    CdsService cds = new CdsService("YourOrgName", "YourAccessToken");

In the above example, replace "YourOrgName" with the organization name for your environment. For example, if your environment URL is https://org109a2adc0.crm.dynamics.com/, your org name is "org109a2adc0".  

Also replace "YourAccessToken" with the access token that you have received after authenticating a user via an OAuth flow. Instructions on how to do this can be found in https://github.com/TimHanewich/Microsoft-Graph-Example/blob/master/CommonDataServiceExample.cs

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