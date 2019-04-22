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

Make sure you have `ruby` and `curl` installed.

To build the SDK and run tests:

        ./build.sh
        
## Using the SDK

To use the SDK, add the [Nuget](https://www.nuget.org/packages/Token.SDK.Net/) package as a dependency to your project file:

<div class="codediv"><pre>
&lt;ItemGroup>
    &lt;PackageReference Include="Token.SDK.Net" Version="1.2.0" />
&lt;/ItemGroup>
</pre></div>
