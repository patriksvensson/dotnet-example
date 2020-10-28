# dotnet example

A dotnet tool to list and run examples similar to `cargo run --example`.

## Installing

```
> dotnet tool install -g dotnet-example
```

## Listing examples

```
> dotnet example

╭─────────┬───────────────────────────────┬────────────────────────────────────────╮
│ Name    │ Path                          │ Description                            │
├─────────┼───────────────────────────────┼────────────────────────────────────────┤
│ Hello   │ examples/First/First.csproj   │ Writes 'Hello World' to the console.   │
│ Goodbye │ examples/Second/Second.csproj │ Writes 'Goodbye World' to the console. │
╰─────────┴───────────────────────────────┴────────────────────────────────────────╯
```

## Running examples

```
> dotnet example hello
Hello World!
```

## Showing example source code

```
> dotnet example table --source
╭────┬────────────────────────────────────────────────╮
│ 1  │ using System;                                  │
│ 2  │                                                │
│ 3  │ namespace First                                │
│ 4  │ {                                              │
│ 5  │     class Program                              │
│ 6  │     {                                          │
│ 7  │         static void Main(string[] args)        │
│ 8  │         {                                      │
│ 9  │             Console.WriteLine("Hello World!"); │
│ 10 │         }                                      │
│ 11 │     }                                          │
│ 12 │ }                                              │
╰────┴────────────────────────────────────────────────╯
```

## Conventions

The convention is simple, if there is an `examples` or `samples` folder 
in the directory the tool is executed in, it will fetch all csproj files 
and find the best match to the query.

## Example settings

To change the name and description of how an example is listed, edit it's `csproj` file, and add the following section:

```csharp
<PropertyGroup>
  <Title>Foo</Title>
  <Description>This is the description of the example.</Description>
</PropertyGroup>
```

If no name is set in the csproj file, the project name will be used.

```
> dotnet example

Foo    This is the description of the example.
```