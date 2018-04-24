# sdk-csharp

C# SDK for interacting with TokenOS.

Account linking and notification features are currently not supported.

## Using the SDK

### On Windows

There are no prerequisites for Windows.

### On Linux and OSX

1. Install `Mono` from [here](https://www.mono-project.com/download/stable/).

    `Mono` is an open source implementation of Microsoft's .NET Framework. It brings the .NET framework to non-Windows envrionments like Linux and OSX.

2. Install `libsodium` library.

    [libsodium](https://github.com/jedisct1/libsodium) is a cross-platform crypto library written in C. The SDK uses a wrapper of the library for `Ed25519` signature generation and verification. 

    **For OSX**:

        brew install libsodium --universal

    **For Linux**:

        xbuild libsodium-net.sln

    For more information, see [here](https://github.com/adamcaudill/libsodium-net).

## Build

To build the SDK and run tests:

        ./build.sh
