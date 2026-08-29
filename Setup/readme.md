# WiX Installer Setup Guide

## Overview

This document describes the setup and build process for creating the **VirtualEMS Windows Installer** using the **WiX Toolset**.

The installer packages the required VirtualEMS applications and published files into a Windows `.msi` installer so that the complete application can be deployed to another Windows machine.

The solution contains multiple application components, including:

* Console application
* Data services
* Web application
* Required DLLs
* Configuration files
* Published application files
* Supporting dependencies

The WiX project is responsible for collecting these published files and packaging them into a single installable application.

---

# 1. Installer Architecture

The overall deployment flow is:

```text
Visual Studio Solution
        │
        ├── Console Application
        │
        ├── Data Services
        │
        └── Web Application
                │
                ▼
          Publish Projects
                │
                ▼
       Published Output Folders
                │
                ▼
             heat.exe
                │
                ▼
          Generated .wxs
                │
                ▼
       Main WiX .wxs Project
                │
                ▼
             WiX Build
                │
                ▼
              .MSI
                │
                ▼
       Windows Installation
```

---

# 2. Prerequisites

Before setting up the installer, install the following:

### Required

* Windows operating system
* Visual Studio
* .NET SDK required by the VirtualEMS projects
* WiX Toolset
* WiX build tools
* `heat.exe`

The WiX version should match the WiX project configuration being used in the solution.

---

# 3. WiX Toolset

WiX is used to create Windows Installer packages.

The important WiX tools used in this project include:

```text
candle.exe
light.exe
heat.exe
```

### candle.exe

Compiles WiX source files:

```text
.wxs → .wixobj
```

### light.exe

Links compiled WiX object files:

```text
.wixobj → .msi
```

### heat.exe

Harvests files from a directory and generates WiX component definitions.

This is particularly useful for applications containing many published files.

---

# 4. Why `heat.exe` Is Used

The VirtualEMS applications generate a large number of files when published.

For example:

```text
VirtualEMS.Web/
│
├── VirtualEMS.Web.dll
├── VirtualEMS.Web.exe
├── appsettings.json
├── web.config
├── *.deps.json
├── *.runtimeconfig.json
├── Newtonsoft.Json.dll
├── Microsoft.*.dll
├── System.*.dll
└── ...
```

Manually writing a WiX `<File>` entry for every file would be difficult and error-prone.

Instead, `heat.exe` can automatically scan the publish directory and generate the required WiX components.

---

# 5. Publishing the Applications

Before running WiX, the applications must be published.

The recommended workflow is:

```text
Clean
  ↓
Build
  ↓
Publish
  ↓
Harvest Published Files
  ↓
Build Installer
```

Example:

```powershell
dotnet publish
```

The exact publish command depends on the project.

A typical publish command may look like:

```powershell
dotnet publish VirtualEMS.Web.csproj -c Release
```

The output is normally placed in a publish directory.

Example:

```text
bin/
└── Release/
    └── net8.0/
        └── publish/
```

---

# 6. Published Output

The installer should package the **published output**, not the normal `bin` build output.

For example:

```text
VirtualEMS.Web/
└── bin/
    └── Release/
        └── net8.0/
            └── publish/
                ├── VirtualEMS.Web.dll
                ├── VirtualEMS.Web.exe
                ├── appsettings.json
                ├── web.config
                └── dependencies...
```

The published directory contains the files required to run the application.

---

# 7. Project Structure

A typical solution structure is:

```text
VirtualEMS/
│
├── VirtualEMS.Console/
│
├── VirtualEMS.DataServices/
│
├── VirtualEMS.Web/
│
└── VirtualEMS.Installer/
    │
    ├── Product.wxs
    ├── Files.wxs
    └── ...
```

The exact project names may vary depending on the current solution.

---

# 8. WiX Installer Project

The WiX installer project contains the main installer definition.

The primary WiX file generally defines:

* Product information
* Package information
* Installation directory
* Features
* Components
* Files
* Shortcuts
* Registry entries
* Application configuration
* Uninstallation behavior

A simplified structure is:

```xml
<Wix>
    <Product>
        <Package />

        <MediaTemplate />

        <Directory>
            ...
        </Directory>

        <Feature>
            ...
        </Feature>
    </Product>
</Wix>
```

The exact syntax depends on the WiX version used by the project.

---

# 9. Product Information

The installer needs basic product information such as:

```text
Product Name
Manufacturer
Version
Upgrade Code
Product Code
```

Example conceptual configuration:

```text
Product Name  : VirtualEMS
Manufacturer  : Company Name
Version       : 1.0.0
```

The product identity should remain consistent between releases.

---

# 10. Installation Directory

The installer must define where VirtualEMS will be installed.

A typical structure is:

```text
C:\Program Files\
    VirtualEMS\
```

Inside the installation directory:

```text
VirtualEMS/
│
├── Web/
├── Services/
├── Console/
└── ...
```

The directory structure should reflect how the application is expected to run after installation.

---

# 11. Features

WiX uses Features to determine what gets installed.

A basic structure is:

```text
VirtualEMS
│
└── Application
    │
    ├── Web Application
    ├── Data Services
    └── Console Application
```

If required, individual features can be made optional.

---

# 12. File Harvesting With `heat.exe`

`heat.exe` is one of the most important parts of this installer setup.

It scans a directory and generates WiX XML containing components and files.

General syntax:

```powershell
heat dir "<PublishFolder>" -out "<OutputFile>.wxs"
```

Example:

```powershell
heat dir "C:\VirtualEMS\VirtualEMS.Web\bin\Release\net8.0\publish" -out "WebFiles.wxs"
```

The generated file contains entries for the discovered files.

---

# 13. Example Harvested Output

A harvested WiX file conceptually contains:

```xml
<Component Id="cmpExample"
           Guid="*">

    <File Id="filExample"
          Source="...\VirtualEMS.Web.dll" />

</Component>
```

For a large application, many such components are generated automatically.

This eliminates the need to manually add every DLL.

---

# 14. Harvesting Multiple Applications

Because VirtualEMS contains multiple applications, each published application can be harvested separately.

Example:

```text
Console Publish
       ↓
heat.exe
       ↓
ConsoleFiles.wxs

Data Services Publish
       ↓
heat.exe
       ↓
ServiceFiles.wxs

Web Publish
       ↓
heat.exe
       ↓
WebFiles.wxs
```

The generated files can then be included in the main installer project.

---

# 15. Important: Harvest the Publish Folder

The correct directory to harvest is the final published directory.

Correct:

```text
bin\Release\netX.X\publish
```

Avoid harvesting:

```text
bin\Debug
bin\Release
obj
source project directory
```

The publish directory contains the deployment-ready files.

---

# 16. Relative Paths vs Absolute Paths

It is preferable to keep installer source paths manageable and reproducible.

Avoid permanently hardcoding machine-specific paths such as:

```text
C:\Users\Username\Desktop\VirtualEMS\...
```

Instead, use project-relative paths or WiX variables where possible.

This makes the installer easier to build on another development machine.

---

# 17. Generated WiX Files

The generated `.wxs` file should generally be treated as a generated artifact.

For example:

```text
WebFiles.wxs
ServiceFiles.wxs
ConsoleFiles.wxs
```

Whenever the application publish output changes significantly, regenerate the corresponding harvested WiX file.

---

# 18. Why Regeneration Is Required

Suppose a new DLL is added to the web application.

Before regeneration:

```text
Published Folder
    ├── Existing.dll
    └── NewLibrary.dll
```

If the existing harvested `.wxs` file does not contain `NewLibrary.dll`, the installer will not include it.

Therefore:

```text
Publish
    ↓
Harvest again
    ↓
Build MSI
```

should be performed whenever deployment files change.

---

# 19. Main WiX File

The main WiX file should coordinate the installer.

Conceptually:

```text
Product.wxs
    │
    ├── Installation Directory
    │
    ├── Product Information
    │
    ├── Features
    │
    ├── WebFiles.wxs
    │
    ├── ServiceFiles.wxs
    │
    └── ConsoleFiles.wxs
```

The harvested files provide the actual file/component definitions.

The main file controls the overall installer.

---

# 20. Component GUIDs

WiX components require component identifiers/GUIDs depending on the WiX version and configuration.

Harvested components are normally generated automatically.

Avoid manually changing generated component identifiers unless there is a specific reason.

Changing component identity incorrectly can cause:

* Upgrade problems
* Repair problems
* Uninstallation problems
* Duplicate files
* Orphaned files

---

# 21. Application Configuration Files

Configuration files must also be included in the installer.

Examples:

```text
appsettings.json
appsettings.Production.json
web.config
```

Be careful with environment-specific configuration.

The installer should not accidentally package development-only configuration as production configuration.

---

# 22. Connection Strings

The VirtualEMS application may depend on database connection strings.

For example:

```text
ConfigDBConnString
```

The installer should not assume that a development machine's database configuration will work on the target machine.

Configuration should be handled appropriately during deployment.

Possible approaches include:

```text
Installer configuration
        ↓
Application configuration
        ↓
Database connection
```

or configuring the application after installation.

---

# 23. Configuration Database

The VirtualEMS application may use a configuration database containing information such as:

```text
Page configuration
Device configuration
Tag configuration
Calculation configuration
Field devices
```

The installer packages the application binaries, but the database itself may require a separate deployment/migration process depending on the environment.

---

# 24. Data Database

The application may also communicate with a separate data database containing:

```text
Live values
Analog data
Historical values
Run hours
Analytics data
```

The installer should therefore be considered the **application deployment package**, not automatically the complete database deployment system unless database installation scripts have explicitly been included.

---

# 25. Required DLLs

One of the main reasons for using harvesting is to ensure all required dependencies are packaged.

Examples can include:

```text
Microsoft.*
System.*
Newtonsoft.*
EntityFrameworkCore.*
Project-specific DLLs
```

The exact DLL list depends on the published application.

Do not manually remove DLLs from the harvested output unless you have verified that they are unnecessary.

---

# 26. Native Dependencies

Some .NET libraries may contain native binaries.

Examples include files such as:

```text
*.dll
*.exe
*.json
*.dat
```

The installer must preserve the published directory structure.

Removing or moving these files can cause runtime failures.

---

# 27. Preserving Directory Structure

If `heat.exe` generates nested directories, those directories must be preserved.

For example:

```text
publish/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
│
├── runtimes/
│   ├── win-x64/
│   └── ...
│
└── application files
```

The installer must reproduce this structure on the target machine.

This is especially important for ASP.NET Core applications.

---

# 28. Web Application Deployment

For the VirtualEMS web application, the published files may include:

```text
wwwroot/
Views/
Pages/
appsettings.json
web.config
*.dll
*.deps.json
*.runtimeconfig.json
```

All required files must be included.

If the application is hosted through IIS, the installer may also need to configure:

* IIS site
* Application pool
* Physical path
* Bindings
* Permissions
* Hosting environment

These are separate installer concerns from simply packaging the published files.

---

# 29. Data Services Deployment

Data services should similarly be published before harvesting.

Example:

```text
DataServices/
└── publish/
    ├── Service.dll
    ├── dependencies...
    └── configuration...
```

The published service files can then be harvested using `heat.exe`.

If the application runs as a Windows Service, the installer may additionally need to create and configure the Windows Service.

---

# 30. Console Application Deployment

The console application can also be published:

```text
ConsoleApp/
└── publish/
    ├── ConsoleApp.exe
    ├── ConsoleApp.dll
    └── dependencies...
```

These files can be harvested and included in the installer.

If the console application is intended to run automatically, an appropriate startup/service mechanism must be configured separately.

---

# 31. Building the Installer

The general build process is:

```text
1. Clean solution
2. Build projects
3. Publish projects
4. Run heat.exe
5. Build WiX project
6. Generate MSI
7. Test installation
```

---

# 32. Recommended Build Sequence

Use the following sequence whenever creating a new installer:

```text
┌─────────────────────────────┐
│ Clean existing output      │
└──────────────┬──────────────┘
               ▼
┌─────────────────────────────┐
│ Build VirtualEMS projects  │
└──────────────┬──────────────┘
               ▼
┌─────────────────────────────┐
│ Publish applications       │
└──────────────┬──────────────┘
               ▼
┌─────────────────────────────┐
│ Run heat.exe               │
└──────────────┬──────────────┘
               ▼
┌─────────────────────────────┐
│ Build WiX project          │
└──────────────┬──────────────┘
               ▼
┌─────────────────────────────┐
│ Generate VirtualEMS.msi    │
└─────────────────────────────┘
```

---

# 33. Example `heat.exe` Workflow

For a published directory:

```text
C:\VirtualEMS\Web\publish
```

run:

```powershell
heat dir "C:\VirtualEMS\Web\publish" -out "WebFiles.wxs"
```

For another application:

```powershell
heat dir "C:\VirtualEMS\DataServices\publish" -out "ServiceFiles.wxs"
```

And for the console application:

```powershell
heat dir "C:\VirtualEMS\Console\publish" -out "ConsoleFiles.wxs"
```

The exact command-line options should match the WiX version and existing project configuration.

---

# 34. WiX Build

After the `.wxs` files are ready, build the WiX project from Visual Studio or the appropriate WiX command-line/build system configured for the project.

The output should contain an MSI such as:

```text
bin/
└── Release/
    └── VirtualEMS.msi
```

The exact output location depends on the project configuration.

---

# 35. Installer Testing

Never assume that a successfully generated MSI means the application is correctly deployed.

Test the MSI on a clean/test machine.

Recommended process:

```text
Install MSI
    ↓
Verify files
    ↓
Verify configuration
    ↓
Start applications/services
    ↓
Verify database connection
    ↓
Open web application
    ↓
Verify SCADA functionality
```

---

# 36. Verify Installed Files

After installation, check the installation directory.

For example:

```text
C:\Program Files\VirtualEMS\
```

Verify that all expected applications and dependencies exist.

Compare:

```text
Published Folder
        vs
Installed Folder
```

Missing files are a common cause of deployment failures.

---

# 37. Verify Web Application

If the Web application is hosted through IIS:

1. Open IIS Manager.
2. Verify the VirtualEMS site.
3. Verify the application pool.
4. Verify the physical path.
5. Verify bindings.
6. Start the site.
7. Open the application in a browser.

Check application logs if the site fails to start.

---

# 38. Verify Services

If VirtualEMS data services are installed as Windows Services:

Open:

```text
services.msc
```

Verify that the required service exists.

Check:

```text
Status
Startup Type
Service Account
Executable Path
```

Start the service and verify that it remains running.

---

# 39. Verify Database Connectivity

After installation, verify that the application can connect to the required databases.

Check:

```text
Configuration Database
Data Database
```

If the application cannot connect:

```text
Check connection string
        ↓
Check SQL Server availability
        ↓
Check credentials
        ↓
Check firewall
        ↓
Check database name
        ↓
Check application configuration
```

---

# 40. Upgrade Testing

The MSI should also be tested for upgrades.

Example:

```text
VirtualEMS 1.0
      ↓
Install
      ↓
VirtualEMS 1.1
      ↓
Upgrade
```

Verify that:

* New files are installed.
* Updated files replace old files.
* Removed files are handled correctly.
* Configuration is preserved as intended.
* Services continue working.
* IIS configuration remains correct.
* The previous version is no longer incorrectly registered.

---

# 41. Uninstallation Testing

Test the uninstall process:

```text
Settings / Control Panel
        ↓
Apps / Programs
        ↓
VirtualEMS
        ↓
Uninstall
```

After uninstalling, verify that the expected application files are removed.

Be careful with:

* User-created files
* Database files
* Logs
* Configuration files
* External application data

These may intentionally need to remain after uninstall.

---

# 42. Common WiX Problems

## 42.1 File Not Found

Error example:

```text
The system cannot find the file specified.
```

Usually means the `Source` path in the WiX file does not point to an existing published file.

Solution:

```text
Check publish output
        ↓
Check Source path
        ↓
Regenerate harvested file
        ↓
Build again
```

---

# 43. Duplicate Components

If the same file is included multiple times, WiX may report component/file conflicts.

Check whether:

```text
WebFiles.wxs
ServiceFiles.wxs
ConsoleFiles.wxs
```

are accidentally harvesting overlapping directories.

Each application should normally have its own distinct published directory.

---

# 44. Missing DLL After Installation

If the application works from the publish directory but fails after installation:

```text
Works before installer
        ↓
Fails after installation
```

check whether the required DLL was included in the harvested WiX file.

Compare:

```text
publish directory
vs
installed directory
```

---

# 45. Application Works in Visual Studio but Not After Installation

This usually indicates an environment/deployment difference.

Check:

```text
Configuration file
Database connection
Working directory
File permissions
Installed runtime
IIS configuration
Windows Service configuration
Environment variables
```

The development environment may contain dependencies that are not available on the target machine.

---

# 46. Stale Harvested Files

One common problem is rebuilding the installer without regenerating the harvested `.wxs` file.

Example:

```text
Developer adds new DLL
        ↓
dotnet publish
        ↓
New DLL exists
        ↓
Old WebFiles.wxs remains
        ↓
MSI does not contain new DLL
```

Solution:

```text
Publish
   ↓
Run heat.exe again
   ↓
Build MSI
```

---

# 47. Recommended Folder Structure

A clean development setup can look like:

```text
VirtualEMS/
│
├── Applications/
│   │
│   ├── Console/
│   │   └── publish/
│   │
│   ├── DataServices/
│   │   └── publish/
│   │
│   └── Web/
│       └── publish/
│
└── Installer/
    │
    ├── Product.wxs
    ├── ConsoleFiles.wxs
    ├── ServiceFiles.wxs
    └── WebFiles.wxs
```

The exact layout can be adjusted to match the existing solution.

---

# 48. Release Checklist

Before delivering a new VirtualEMS installer:

## Applications

* [ ] Console application builds.
* [ ] Data services build.
* [ ] Web application builds.
* [ ] All projects publish successfully.

## Publish Output

* [ ] Publish folders contain all required files.
* [ ] Configuration files are correct.
* [ ] No development-only files are unintentionally included.

## WiX

* [ ] `heat.exe` has been executed for updated publish folders.
* [ ] Generated `.wxs` files are updated.
* [ ] No duplicate files/components exist.
* [ ] Installation directory is correct.
* [ ] Product version is updated.
* [ ] Installer builds successfully.

## Installation

* [ ] MSI installs successfully.
* [ ] All expected files are installed.
* [ ] Web application works.
* [ ] Data services work.
* [ ] Console application works.
* [ ] Database connectivity works.
* [ ] Required permissions are available.

## Upgrade

* [ ] Previous version upgrades correctly.
* [ ] New files are installed.
* [ ] Old files are handled correctly.
* [ ] Services continue running.
* [ ] IIS configuration remains valid.

## Uninstall

* [ ] MSI uninstalls successfully.
* [ ] Application files are removed as expected.
* [ ] Required persistent data is preserved.

---

# 49. Quick Reference

### Publish

```powershell
dotnet publish <project>.csproj -c Release
```

### Harvest

```powershell
heat dir "<publish-folder>" -out "<generated-file>.wxs"
```

### Build

```text
Build WiX Project
        ↓
WiX Compilation
        ↓
MSI Generation
```

### Test

```text
Install
 ↓
Run
 ↓
Verify
 ↓
Upgrade
 ↓
Uninstall
```

---

# 50. Final Deployment Flow

The complete VirtualEMS installer process is:

```text
                  VIRTUAL EMS SOLUTION
                           │
          ┌────────────────┼────────────────┐
          │                │                │
          ▼                ▼                ▼
      Console         Data Services        Web
          │                │                │
          ▼                ▼                ▼
       Publish           Publish          Publish
          │                │                │
          ▼                ▼                ▼
      publish/           publish/         publish/
          │                │                │
          └────────────────┼────────────────┘
                           │
                           ▼
                       heat.exe
                           │
          ┌────────────────┼────────────────┐
          ▼                ▼                ▼
    ConsoleFiles.wxs  ServiceFiles.wxs  WebFiles.wxs
          │                │                │
          └────────────────┼────────────────┘
                           ▼
                       Product.wxs
                           │
                           ▼
                       WiX Build
                           │
                           ▼
                    VirtualEMS.msi
                           │
                           ▼
                       Install
                           │
          ┌────────────────┼────────────────┐
          ▼                ▼                ▼
       Console        Data Services        Web
          │                │                │
          └────────────────┼────────────────┘
                           ▼
                    VirtualEMS System
```

---

# 51. Important Rule

The most important rule for maintaining the VirtualEMS installer is:

> **Always build the installer from the latest published application output.**

Whenever application dependencies or published files change:

```text
Code Change
    ↓
Build
    ↓
Publish
    ↓
Run heat.exe
    ↓
Build WiX
    ↓
Generate MSI
    ↓
Test Installation
```

Do not rely on an old harvested `.wxs` file when the published application has changed.

---

# 52. Summary

The VirtualEMS WiX installer provides a repeatable method for packaging the complete application for Windows deployment.

The core process is:

```text
Build Applications
        ↓
Publish Applications
        ↓
Harvest Published Files
        ↓
Generate/Update WiX Components
        ↓
Build WiX Installer
        ↓
Generate MSI
        ↓
Install and Test
```

The three key parts are:

```text
1. Published Application Files
2. Harvested WiX Components
3. Main WiX Installer Definition
```

`heat.exe` is particularly important because it automatically generates the file/component definitions required to package the large number of DLLs and supporting files generated by the VirtualEMS applications.

For every release, the safest workflow is to **publish first, regenerate the harvested WiX files, build the MSI, and then test the MSI on a clean environment**.

