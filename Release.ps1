dotnet clean

dotnet publish -c:Release /p:DebugType=None /p:DebugSymbols=false -r linux-arm
tar -C bin/Release/net7.0/linux-arm/publish/ -cvzf zips/linux-arm.tar.gz *

dotnet publish -c:Release /p:DebugType=None /p:DebugSymbols=false -r linux-arm64
tar -C bin/Release/net7.0/linux-arm64/publish/ -cvzf zips/linux-arm64.tar.gz *

dotnet publish -c:Release /p:DebugType=None /p:DebugSymbols=false -r linux-x64
tar -C bin/Release/net7.0/linux-x64/publish/ -cvzf zips/linux-x64.tar.gz *

dotnet publish -c:Release /p:DebugType=None /p:DebugSymbols=false -r osx-x64
tar -C bin/Release/net7.0/osx-x64/publish/ -cvzf zips/osx-x64.tar.gz *

dotnet publish -c:Release /p:DebugType=None /p:DebugSymbols=false -r osx.13-arm64
tar -C bin/Release/net7.0/osx.13-arm64/publish/ -cvzf zips/osx.13-arm64.tar.gz *

dotnet publish -c:Release /p:DebugType=None /p:DebugSymbols=false -r win-arm
Compress-Archive -Path bin/Release/net7.0/win-arm/publish/* zips/win-arm.zip

dotnet publish -c:Release /p:DebugType=None /p:DebugSymbols=false -r win-arm64
Compress-Archive -Path bin/Release/net7.0/win-arm64/publish/* zips/win-arm64.zip

dotnet publish -c:Release /p:DebugType=None /p:DebugSymbols=false -r win-x64
Compress-Archive -Path bin/Release/net7.0/win-x64/publish/* zips/win-x64.zip

dotnet publish -c:Release /p:DebugType=None /p:DebugSymbols=false -r win-x86
Compress-Archive -Path bin/Release/net7.0/win-x86/publish/* zips/win-x86.zip
