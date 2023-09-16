# Humanization Steps
- Remove null values
- Trim unneeded/internal fields
    - OData fields
    - state code
    - status code
    - Primary key (GUID)
- Property names to display names
- Choices to names
- Format dates
- Depth

## Original
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

## Step 1: Remove Null Values
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
}
```

## Step 2: Trim unneeded fields
```
{
  "cra0f_favoritespecies": "238080000,238080001,238080002",
  "cra0f_dateofbirth": "2022-09-01T00:00:00Z",
  "_owningbusinessunit_value": "5bd9dba0-c95c-ed11-9562-000d3a1df4a2",
  "_cra0f_homepen_value": "95fc11dc-2fc7-ed11-b597-000d3a8c2011",
  "cra0f_species": 238080001,
  "_cra0f_mother_value": "4f6a9a15-30c7-ed11-b597-000d3a8c2011",
  "_createdby_value": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "_ownerid_value": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "modifiedon": "2023-09-16T20:18:46Z",
  "_owninguser_value": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "_modifiedby_value": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "cra0f_name": "Phillip Piggy",
  "createdon": "2023-03-20T15:01:57Z",
}
```

## Step 3: Logic Names to Display Names
```
{
  "Favorite Species": "238080000,238080001,238080002",
  "Date of Birth": "2022-09-01T00:00:00Z",
  "Owning Business Unit": "5bd9dba0-c95c-ed11-9562-000d3a1df4a2",
  "Home Pen": "95fc11dc-2fc7-ed11-b597-000d3a8c2011",
  "Species": 238080001,
  "Mother": "4f6a9a15-30c7-ed11-b597-000d3a8c2011",
  "Created By": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Owner": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Modified On": "2023-09-16T20:18:46Z",
  "Owning User": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Modified By": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Name": "Phillip Piggy",
  "Created On": "2023-03-20T15:01:57Z",
}
```

## Step 4: Choice Values to Choice Labels
```
{
  "Favorite Species": "Cow; Pig; Chicken",
  "Date of Birth": "2022-09-01T00:00:00Z",
  "Owning Business Unit": "5bd9dba0-c95c-ed11-9562-000d3a1df4a2",
  "Home Pen": "95fc11dc-2fc7-ed11-b597-000d3a8c2011",
  "Species": "Pig",
  "Mother": "4f6a9a15-30c7-ed11-b597-000d3a8c2011",
  "Created By": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Owner": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Modified On": "2023-09-16T20:18:46Z",
  "Owning User": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Modified By": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Name": "Phillip Piggy",
  "Created On": "2023-03-20T15:01:57Z",
}
```

## Step 5: Format Dates
```
{
  "Favorite Species": "Cow; Pig; Chicken",
  "Date of Birth": "9/1/2022 12:00:00 AM",
  "Owning Business Unit": "5bd9dba0-c95c-ed11-9562-000d3a1df4a2",
  "Home Pen": "95fc11dc-2fc7-ed11-b597-000d3a8c2011",
  "Species": "Pig",
  "Mother": "4f6a9a15-30c7-ed11-b597-000d3a8c2011",
  "Created By": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Owner": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Modified On": "9/16/2023 8:18:46 PM",
  "Owning User": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Modified By": "4de0dba0-c95c-ed11-9562-000d3a1df4a2",
  "Name": "Phillip Piggy",
  "Created On": "3/20/2023 3:01:57 PM",
}
```

## Step 6: Relational Depth
```
{
  "Favorite Species": "Cow; Pig; Chicken",
  "Date of Birth": "9/1/2022 12:00:00 AM",
  "Owning Business Unit": {"name": "Sunny Farms"}
  "Home Pen": {
    "Modified On": "3/20/2023 2:59:49 PM",
    "Name": "Wayne Locks",
    "Created On": "3/20/2023 2:59:49 PM"
  },
  "Species": "Pig",
  "Mother": {
    "Date of Birth": "2/16/2023 12:00:00 AM",
    "Species": "Chicken",
    "Name": "Caitlyn Chicky",
  },
  "Created By": {
    "Access Mode": "Read-Write",
    "User Name": "admin@D365DemoTS909196.onmicrosoft.com",
    "Full Name": "System Administrator",
    "License Type": "Enterprise",
  },
  "Owner": {"Full Name": "System Administrator"},
  "Modified On": "9/16/2023 8:18:46 PM",
  "Owning User": {"Full Name": "System Administrator"},
  "Modified By": {"Full Name": "Ken Zigway"},
  "Name": "Phillip Piggy",
  "Created On": "3/20/2023 3:01:57 PM",
}
```