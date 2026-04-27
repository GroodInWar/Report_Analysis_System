# General Information

### Title:
*File Analysis Website - CS-GY 6083 Project*

### Author Information
 - Principal Developer: Gustavo Sakamoto de Toledo (gs4631)

### Project Information
This project is being developed for **CS-GY 6083** - Principles of Database Systems at **New York University (NYU)**. It focuses on the design of a database-backed web application for analyzing user-submitted files and managing security-related incident reports.

#### <u>Project Description</u>
The system is designed as a **three-tier file analysis and incident reporting website**. Its purpose is to allow users to submit reports involving suspicious or relevant files, while analysts and administrators review those reports, classify incidents, assign severity levels, and document related findings.

The application follows a **three-tier architecture**, which separates the system into:
```text
Frontend -> user-facing web interface
Backend  -> server-side logic and processing
Database -> MySQL database storing reports, incidents, files, and user data
```
In this design, users interact with a frontend website that submits requests to the back end. The backend processes these requests and enforces system rules. Finally, the database stores the structured data for reports, incidents, comments, files, and user accounts. 

At a high level, the system is intended to support the following workflow: 

#### <u>E-R Diagram Overview</u>
The E-R diagram provides the conceptual structure of the database for the file analysis website. It models the major entities involved in the system, including Users, Roles, Reports, incidents, Comments, Files, Categories, and Severity.
Each user is assigned a role, such as regular user, analyst, or administrator, which defines level of access in the system. Users can submit reports, and each report may include one or more file. After a review process is conducted by an assigned analyst, the report may lead to the creation of an incident. Each incident is associated with a category and a severity level, which helps organize and prioritize investigations. Users can also write comments on incidents, allowing discussion and documentation throughout the analysis process. Files may be connected both to reports and to incidents, making it possible to track evidence across different stages of the workflow.

The following picture illustrates the E-R model diagram utilizing the notation in the book Database System Concepts - 6th Edition by Abraham Silberschatz, Henry F. Korth, and S. Sudarshan. It also utilizes Boyce-Codd Normal Form (BCNF) to eliminate redundancies, and achieve a more desirable normal form. 

![Database E-R Model Diagram | Database System Concepts](/database_E-R(DSC6th).png)

The following picture illustrates the final diagram of the schema created utilizing the previous diagram. It shows entity attributes with corresponding datatypes. 

![Database Schema Diagram | Datagrip](/database_E-R_diagram(datatypes).png)

##