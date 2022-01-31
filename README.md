# Pglet client for .NET - quickly build interactive web apps in C#, F# or VB.NET

[Pglet](https://pglet.io) is a rich User Interface (UI) framework to quickly build interactive web apps in .NET without prior knowledge of web technologies like HTTP, HTML, CSS or JavaSscript. You build UI with [controls](https://pglet.io/docs/reference/controls) which use [Fluent UI React](https://developer.microsoft.com/en-us/fluentui#/controls/web) to ensure your programs look cool and professional.

## Requirements

* .NET Framework 4.6.1 or above
* .NET Core 5 or above on Windows, Linux or macOS

## Installation

Add `pglet` package to your project:

```
dotnet add package Pglet
```

## Hello, world!

Create a new console application:

```
mkdir hello-world
cd hello-world
dotnet new console
```

Add `Pglet` package to your project:

```
dotnet add package Pglet
```

Update `Program.cs` with:

```csharp
using Pglet;
var page = await PgletClient.ConnectPage();
await page.AddAsync(new Text { Value = "Hello, world!" });
```

Run the app:

```
dotnet run
```

A new browser window will popup:

![Sample app in a browser](https://pglet.io/img/docs/quickstart-hello-world.png "Sample app in a browser")

Here is a local page served by an instance of Pglet server started in the background on your computer.

## Make it web

Add `web: true` parameter to `ConnectPage()` call:

```csharp
using Pglet;
var page = await PgletClient.ConnectPage(web: true);
await page.AddAsync(new Text { Value = "Hello, world!" });
```

This time page will be created on [Pglet hosted service](https://pglet.io/docs/pglet-service).