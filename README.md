# Pglet client for PowerShell

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

Create a new `hello.ps1` with the following content:

```posh
TODO
```

Run `hello.ps1` in your PowerShell session and in a new browser window you'll get:

![Sample app in a browser](https://pglet.io/img/docs/quickstart-hello-world.png "Sample app in a browser")

Here is a local page served by an instance of Pglet server started in the background on your computer.

## Make it web

Add `-Web` parameter to `Connect-PgletPage` call:

```posh
Connect-PgletPage -Web
Invoke-Pglet "add text value='Hello, world!'"
```

This time page will be created on [Pglet hosted service](https://pglet.io/docs/pglet-service).

Read [PowerShell tutorial](https://pglet.io/docs/tutorials/powershell) for further information and more examples.