Mumble.NET
==========
Mumble.NET is an [open source library][1] that provides a class library that can communicate with a Mumble VoIP server.

### Features

  * Connect to any Mumble VoIP server programatically
  * Move between channels
  * Send text messages to users or channels
  * Respond to messages or user state changes with callbacks
  * Transmit audio data

### Installation

Add a dependency to this library in your project via the pre-built NuGet package

    Install-Package Mumble.NET

### Documentation

```csharp
using Mumble;

class MumbleClient
{
    private static void Main(string[] args)
    {
        // TBD
    }
}
```

### Building

From the root of the repository:

    msbuild

If you are making a pull request, please do a command line build first. StyleCop and FXCop warnings are run with command line builds.

[1] https://github.com/perrym5/Mumble.NET
