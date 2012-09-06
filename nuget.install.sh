#!/bin/bash
rm -rf packages
mono --runtime=v4.0 ../work-tool/NuGet.exe install src/Cooper.Model/packages.config -o packages -NoCache -source http://cs-nuget.apphb.com/nuget;http://nuget.org/api/v2/
mono --runtime=v4.0 ../work-tool/NuGet.exe install src/Cooper.Model.Mappings/packages.config -o packages -NoCache -source http://cs-nuget.apphb.com/nuget;http://nuget.org/api/v2/
mono --runtime=v4.0 ../work-tool/NuGet.exe install src/Cooper.Model.Test/packages.config -o packages -NoCache -source http://cs-nuget.apphb.com/nuget;http://nuget.org/api/v2/
mono --runtime=v4.0 ../work-tool/NuGet.exe install src/Cooper.Repositories/packages.config -o packages -NoCache -source http://cs-nuget.apphb.com/nuget;http://nuget.org/api/v2/
mono --runtime=v4.0 ../work-tool/NuGet.exe install src/Cooper.Web/packages.config -o packages -NoCache -source http://cs-nuget.apphb.com/nuget;http://nuget.org/api/v2/