# dotnet-nugit

**dotnet nugit** is a command-line interface (CLI) that can build NuGet packages on the fly from sources available on GitHub and add them as package references to projects.  You may have already guessed it: the term **nugit** is a wordplay between NuGet and Git, which seems to be the perfect and one-and-only name that makes sense here :-)


## Build

### Prerequisites

The project requires the following tools and frameworks to be installed:

* .NET Framework 8.0 SDK
* An editor of choice; Visual Studio Code + C# Dev Kit is fine


The console application project is dependent on external code available on GitHub, which is integrated into the repo using submodules.  Run the following command to fetch submodules before trying to build the application.

````bash
$ git submodule update --init --recursive
````

Referencing external code via submodules works okay, but wouldn´t it be nice to just reference the repo, as we do with NuGet packages? Well, this is the aim of this project, though.

### Package the application

The project file is already prepared to package and distribute the application as a tool. The installation package can be created using the `dotnet pack` command:

````bash
$ cd src/dotnet.nugit
$ dotnet pack
````

## Installation

Install the tool from the package by running the `dotnet tool install` command in the dotnet.nugit project folder:

````bash
$ cd src/dotnet.nugit
$ dotnet tool install --global ./nupkg/dotnet.nugit.1.0.0.nupkg
````

## Usage

Once installed, the tool can be invoked by the `dotnet nugit` command; the followimg sub-commands are supported:


| Command | Flag | Description |
| - | - | - |
| `env` |   | Lists environment variables and their values. |
| `init`    | | Adds a `.nugit` file, which holds metadata to referenced repositories.  Creates a new local NuGet feed to integrate packages with other tools like `dotnet add package`. |
|           | `--local` | Configures nugit to put copies of nupkg files into the current workspace. |
| `restore` | | Fetches, builds and publishes packages from repos to the local nugit. |
| `update`  | | Fetches changes from referenced repositories (if not referenced by tag or commit-hash), rebuilds and publishes packages. |
| `list`    | | Lists available packages available in a repository. |
| `add`     | | Builds a package from a referenced repository and publishes it to the local feed. |
| `tidy`    | | Purges all packages from the local feed that are not referenced by the local workspace. |