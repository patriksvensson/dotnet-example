# dotnet example

A dotnet tool to list and run examples similar to `cargo run --example`.

## Installing

```
> dotnet tool install -g dotnet-example
```

## Listing examples

```
> dotnet example

Colors    Demonstrates how to use colors in the console.
Grid      Demonstrates how to render grids in a console.
Table     Demonstrates how to render items in panels.
Table     Demonstrates how to render tables in a console.
```

## Running examples

```
> dotnet example table
┌──────────┬──────────┬────────┐
│ Foo      │ Bar      │ Baz    │
├──────────┼──────────┼────────┤
│ Hello    │ World!   │        │
│ Bounjour │ le       │ monde! │
│ Hej      │ Världen! │        │
└──────────┴──────────┴────────┘
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