nuget install NUnit.Runners -Version 2.6.4 -OutputDirectory tools
nuget install OpenCover -Version 4.6.519 -OutputDirectory tools
nuget install coveralls.net -Version 0.412.0 -OutputDirectory tools
nuget install Codecov -Version 1.0.1 -OutputDirectory tools
 
.\tools\OpenCover.4.6.519\tools\OpenCover.Console.exe -target:.\tools\NUnit.Runners.2.6.4\tools\nunit-console.exe -targetargs:"/nologo /noshadow .\hcl-net.Test\bin\Debug\hcl-net.Test.dll /framework:net-4.0" -filter:"+[*]* -[*.Test]*" -register:user

.\tools\coveralls.net.0.412\tools\csmacnz.Coveralls.exe --opencover -i .\results.xml 
.\tools\Codecov.1.0.1\tools\codecov.exe -f ".\results.xml"
