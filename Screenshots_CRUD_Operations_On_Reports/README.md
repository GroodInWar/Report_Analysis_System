# Simple UI for a Single Table
> This portion of the project explores the creation of a User Interface (UI) for a single table.

## Source Code
> The source code is separated into many .NET projects.
- client (Front-End program): Creates a User Interface that the will interact with to communicate with the server. Utilizes Blazor (You can find the exact tutorial I used to create the base for this portion at this link [Blazor Tutorial](https://dotnet.microsoft.com/en-us/learn/aspnet/blazor-tutorial/intro)).
- server (Back-End Program): Creates the HTTP API backend that handles requests/resposes from/to the client application. The backend utilizes ASP.NET Core (You can find the link to the tutorial and documentation pages at this link [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-10.0&WT.mc_id=dotnet-35129-website&tabs=visual-studio))
- Shared (Model and Database Context Classes): It stores the classes that the database is managing. You can also find the tutorial in the same link as that details the server handling. 
- server.Tests (Automated tests): I'm still working on how to handle this requests to automate the testing process. So far, I can test that the program can connect to the database and process requests/responses. But, I still have to work on creating a way to automate tests on the HTTP request/response on server/client. 
> All of these descriptions will be better developed for the official README.md file at the root folder of the project.

## CRUD on Reports Table Process
### Create
> A user creates a report by clicking the ``New Report`` button at the top left of the main Reports tab. 
![Client Create](/Screenshots_CRUD_Operations_On_Reports/client_INSERT.png)
> The client program attaches the user_id, since users shouldn't choose their id's. Once, the user saves it, an HTTP POST request is sent to the backend server, and processed to the database as shown below.
![Server INSERT](/Screenshots_CRUD_Operations_On_Reports/server_INSERT.png)

### Read
> At any time an update to the Reports table is done, a new HTTP GET will be sent from the client in response to a HTTP 200 code. 
![Client Read](/Screenshots_CRUD_Operations_On_Reports/client_SELECT(empty).png)
> Here the user has an empty relation resulted from their SELECT operation in the backend. You can see the SELECT operation below:
![Server SELECT](/Screenshots_CRUD_Operations_On_Reports/server_SELECT(empty).png)

### Update
> Once a Report instance is created in the database, we will see it in the table display in the client UI. To update, the user simply has to click `Edit` and modify either its name or description. Note, that the user cannot change the attributes: `user_id`, `report_id`, `created_at`, and `updated_at`. These attributes are updated by the database and client, the backend only confirms the correctness of the data submitted before processing it. When the user `Save`s the changes, the client program submits an HTTP PUT query to the server.
![Client Edit](/Screenshots_CRUD_Operations_On_Reports/client_UPDATE.png)
> You can see in the picture below the result operation on the database processing the editing on the Report instance as an UPDATE query.
![Server UPDATE](/Screenshots_CRUD_Operations_On_Reports/server_UPDATE.png)

### DELETE
> To delete a report instance a user clicks the `Delete` button as shown in the picture below. That sends a HTTP DELETE query with the `id` of the report referenced by this `id`. 
![Client Delete](/Screenshots_CRUD_Operations_On_Reports/client_DELETE.png)
> The server process it as a DELETE query to the underlying database as shown below.
![Server Delete](/Screenshots_CRUD_Operations_On_Reports/server_DELETE.png)