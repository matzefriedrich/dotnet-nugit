# dotnet-nugit

**dotnet nugit** is a command-line interface (CLI) that can build NuGet packages on the fly from sources available on GitHub and add them as package references to projects.

## Prerequisites

The project requires the following tools and frameworks to be installed:

* .NET Framework 8.0 SDK
* An editor of choice; Visual Studio Code + C# Dev Kit is fine


The console application project is dependent on external code available on GitHub, which is integrated into the repo using submodules.  Run the following command to fetch submodules before trying to build the application.

````bash
$ git submodule update --init --recursive
````

Referencing external code via submodules works okay, but wouldn´t it be nice to just reference the repo, as we do with NuGet packages? Well, this is the aim of this project, though.
