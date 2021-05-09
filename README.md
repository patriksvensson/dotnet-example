# dotnet example

A dotnet tool to list and run examples similar to Rust's `cargo run --example`.

## Installing

```
> dotnet tool install -g dotnet-example
```

## Listing examples

```
> dotnet example

╭─────────────┬───────────────────────────────────────────────╮
│ Example     │ Description                                   │
├─────────────┼───────────────────────────────────────────────┤
│ CSharp      │ Writes 'Hello world C#' to the console        │
│ FSharp      │ Writes 'Hello world F#' to the console        │
╰─────────────┴───────────────────────────────────────────────╯

Type dotnet example --help for help
```

## Running examples

```
> dotnet example csharp
Hello world from C#

> dotnet example fsharp
Hello world from F#
```

## Showing example source code

```
> dotnet example fsharp --source

╭────┬───────────────────────────────────────────────────────────────────╮
│ 1  │ // Learn more about F# at http://docs.microsoft.com/dotnet/fsharp │
│ 2  │                                                                   │
│ 3  │ open System                                                       │
│ 4  │                                                                   │
│ 5  │ // Define a function to construct a message to print              │
│ 6  │ let from whom =                                                   │
│ 7  │     sprintf "from %s" whom                                        │
│ 8  │                                                                   │
│ 9  │ [<EntryPoint>]                                                    │
│ 10 │ let main argv =                                                   │
│ 11 │     let message = from "F#" // Call the function                  │
│ 12 │     printfn "Hello world %s" message                              │
│ 13 │     0 // return an integer exit code                              │
╰────┴───────────────────────────────────────────────────────────────────╯
```

## Conventions

The convention is simple, if there is an `examples` or `samples` folder 
in the directory the tool is executed in, it will fetch all `csproj`/`fsproj` files 
and find the best match to the query.

If examples are located in unconventional folders, add a `.examples` file
with the (relative) paths of the examples folders, one per line. Blank lines
or lines starting with `#` in the `.examples` file are ignored.

## Example settings

To change the name, description, and the order of an example, edit its `csproj`/`fsproj` file, and add the following section:

```xml
<PropertyGroup>
  <ExampleTitle>Foo</ExampleTitle>
  <ExampleDescription>This is the description of the example.</ExampleDescription>
  <ExampleOrder>5</ExampleOrder>
</PropertyGroup>
```

If no name is set in the `csproj` file, the project name will be used.
To ignore an example, add the `ExampleVisible` property in the example's `csproj`/`fsproj` file.

```xml
<PropertyGroup>
  <ExampleVisible>false</ExampleTitle>
</PropertyGroup>
```