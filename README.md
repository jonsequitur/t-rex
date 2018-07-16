# t-rex

A command line tool for testing with `dotnet`.

[![Build Status](https://ci.appveyor.com/api/projects/status/github/jonsequitur/t-rex?svg=true&branch=master)](https://ci.appveyor.com/project/jonsequitur/t-rex)
 [![NuGet Status](http://img.shields.io/nuget/v/t-rex.svg?style=flat)](https://www.nuget.org/packages/t-rex/) 

With `t-rex`, you can explore the results of your most recent test run. 

First, run:

```
dotnet test -l trx
```

Then run `t-rex`:

![image](https://user-images.githubusercontent.com/547415/42780528-4dc689e2-88f8-11e8-9294-07775dee0695.png)

`t-rex` discovers and parses `.trx` files, considering only the most recent ones in a given folder.

```
> t-rex.exe -h
t-rex:
  A command line testing tool for .NET

Usage:
  t-rex [options]

Options:
  --file                .trx file(s) to parse
  --filter              Only look at tests matching the filter. "*" can be  
                        used as a wildcard.
  --path                Directory or directories to search for .trx files. 
                        Only the most recent .trx file in a
                        given directory is used.
  --show-test-output    For failed tests, display the output.
  --version             Display version information
```





If you are using the [`2.1.300`](https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.300) version of `dotnet` or later, you can install `t-rex` from the command line using:

```shell
dotnet tool install -g t-rex 
```

