# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- [PR #8] The application can access package configuration properties directly from project files and override/tweak them as needed. The `add` and `restore` commands now support the `--configuration` flag, as known from other `dotnet` commands. (#7)
- Adds support for F# project files (#7)

### Changed

- [PR #8] Changes services involved with building and packing NuGet packages.
- [PR #8] The `dotnet pack` command is now invoked with the `-p:NuspecFile` option; Nuspec files are dynamically generated from project files (#7)


### Fixed

- [PR #8] Fixes broken task factories

## [0.2.0-alpha1.240419.1] - 2024-04-19

### Added
- [PR #6] Adds the `restore` command that can restore referenced Git repositories and output packages. (#5)

### Changed
- The functionality required to clone and build projects got refactored; service types for common tasks have been added and are now utilized by the `add` and `restore` commands. (#5)
- The tool can now look up the workspace configuration file `.nugit` in the current working directory or parent directories. Commands that require workspace information can now be used from any directory within the repository tree.
- Because lots of things require IO, from now on `System.IO.Abstractions` is used to make IO acess testable
- Support of implicit usings got disabled (namespace imports indicate dependencies and we wanna see them)

### Fixed
- Removes `REQUIRED` constraint from several boolean command-line flags. Opt-in flags are `false` by default, and get set to `true` if specified.

## [0.1.0-alpha1.240417.1] - 2024-04-17

This is the first pre-release version that comes with a basic set of features.

### Added
- [PR #1] Adds the `env` command that lists configuration values
- [PR #2] Adds the `init` command that can create a new workspace and the local NuGet feed.
- [PR #3] Add the `add` command that can clone repositories, find compatible .NET projects. It uses the `dotnet pack` command to build NuGet packages.
- [PR #4] Adds a bunch of tests
- The application can be packed and installed locally as a pre-release version and integrates nicely with the `dotnet` CLI.