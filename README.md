# dotnet example

A dotnet tool to list and run examples.

## Installing

```
> dotnet tool install -g dotnet-example
```

## Listing examples

```
> dotnet example list

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

The convention is simple, if there is an `examples` folder in the directory the
tool is executed in, it will fetch all csproj files and find the best match to
the query.