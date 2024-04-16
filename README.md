![.NET 8](https://github.com/matzefriedrich/dotnet-nugit/actions/workflows/dotnet.yml/badge.svg)


# dotnet-nugit

**dotnet nugit** is a command-line interface (CLI) that can build NuGet packages on the fly from sources available on GitHub and add them to a local NuGet feed. It is helpful when an official NuGet package is unavailable at [nuget.org](https://www.nuget.org/). The tool helps to avoid unofficial package releases but makes it possible to reference projects conveniently. This project aims to create a utility that makes it easy to integrate external .NET projects hosted on a Git repository, similar to what is possible in the Golang ecosystem.

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
$ mkdir build
$ dotnet pack src/dotnet.nugit/dotnet.nugit.csproj --configuration Release -o ./build/
````

## Installation

Install the tool from the package by running the `dotnet tool install` command in the dotnet.nugit project folder:

````bash
$ dotnet tool install --global ./build/dotnet.nugit.0.1.0-alpha1.240417.1.nupkg
````

Once installed, the tool can be invoked by the `dotnet nugit` command; see the next section for a list of supported commands:

## Usage

Use the `--help` option to display a complete list of supported commands on the console.

### Initialize your local workspace

Use the `init` command to add support for `dotnet-nugit` to your local workspace, i.e. your project folder that acts as your Git repository root folder.  The command creates a `.nugit` file that keeps track of package repositories added to your workspace. It will also create a local NuGet feed named `LocalNuGitFeed`, if missing.

````bash
$ cd <your-project-folder>
$ dotnet nugit init
````

### Configuration

The application depends on the local `NuGet.Config` file (this is the place where the local feed gets registered), a `.nugit` workspace file, and a few environment variables. None of those configuration sources require much attention, except you want change where NuGit stores clones repositories, and build packages.

Use the `env` command to get a list of environment variables (and their current values) which can be changed.

````bash
$ dotnet nugit env
````

### Adding a remote repository

Use the `add` command to reference a remote Git repository. The application adds a repository reference to the `.nugit` file, clones the repository into the `$NUGIT_HOME/repositories` directory, tries to find compatible .NET projects, builds and packs them, and lastly, copies the `pkg` files to the local feed. Building and packaging is done through `dotnet build` and `dotnet pack` commands, which are available anyways.

````bash
$ dotnet nugit add \
    --repository https://github.com/matzefriedrich/command-line-api-extensions.git \
    --head-only
````

If the `--head-only` option is set to `false`, it does not only build the head commit version but also enumerates available references (tags) and builds them. This way, a set of packages can automatically be built for each project found in the repository.  The quality of the package metadata depends on the information present in each project, though the NuGit application can try to improve. For instance, it appends a version suffix that either indicates the tag name or the commit sha from which the package was created. 

Once packages have been pushed to the local feed, they can be referenced via the `dotnet add package` command.

---
Copyright (c) 2024 by Matthias Friedrich, published under [AGPL-3.0 license](LICENSE)