# SDK-CSharp

C# TPP SDK for interacting with [Token](https://token.io) Platform.

## Requirements

### On Windows

There are no prerequisites for Windows.

### On Linux and OSX

Install `Mono` from [here](https://www.mono-project.com/download/stable/).

    `Mono` is an open source implementation of Microsoft's .NET Framework. It brings the .NET framework to non-Windows envrionments like Linux and OSX.

## Build

### On OSX

Make sure you have `dotnet-sdk` installed.

To build the solution:

    dotnet build

To run the tests:

    dotnet test

## Using the SDK

To use the SDK, add the [Nuget](https://www.nuget.org/packages/Token.SDK.TPP/) package for TPPs
(or [Token.SDK.User](https://www.nuget.org/packages/Token.SDK.User/) for user-side integrations)
as a dependency to your project file:

<div class="codediv"><pre>
&lt;ItemGroup>
    &lt;PackageReference Include="Token.SDK.TPP" Version="&lt;current version>" />
&lt;/ItemGroup>
</pre></div>

`Token.SDK.Net` is deprecated and no longer maintained. Migrate to `Token.SDK.TPP` or `Token.SDK.User`.
