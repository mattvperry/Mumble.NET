Mumble.NET [![Build status][buildimg]][buildlnk] [![GitHub license][mitimg]][mitimg]
==========
Mumble.NET is an [open source library][selflink] that provides a class library that can communicate with a Mumble VoIP server.

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

    nuget restore src/Mumble.NET.sln
    msbuild

If you are making a pull request, please do a command line build first. StyleCop and FXCop warnings are run with command line builds.

 [selflink]: https://github.com/mattvperry/Mumble.NET
 [buildimg]: https://ci.appveyor.com/api/projects/status/theq3c2x1l64uu5p?svg=true
 [buildlnk]: https://ci.appveyor.com/project/mattvperry/mumble-net
 [mitimg]: https://img.shields.io/badge/license-MIT-blue.svg?style=flat
 [mitlnk]: https://raw.githubusercontent.com/mattvperry/Mumble.NET/master/LICENSE
