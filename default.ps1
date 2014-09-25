properties { 
	$projectName = "Cedar"
	$buildNumber = 0
	$rootDir  = Resolve-Path .\
	$buildOutputDir = "$rootDir\build"
	$reportsDir = "$buildOutputDir\reports"
	$srcDir = "$rootDir\src"
	$solutionFilePath = "$srcDir\$projectName.sln"
	$assemblyInfoFilePath = "$srcDir\SharedAssemblyInfo.cs"
	$ilmerge_path = "$srcDir\packages\ILMerge.2.13.0307\ILMerge.exe"
}

task default -depends Clean, UpdateVersion, RunTests, CreateNuGetPackages

task Clean {
	Remove-Item $buildOutputDir -Force -Recurse -ErrorAction SilentlyContinue
	exec { msbuild /nologo /verbosity:quiet $solutionFilePath /t:Clean }
}

task UpdateVersion {
	$version = Get-Version $assemblyInfoFilePath
	$oldVersion = New-Object Version $version
	$newVersion = New-Object Version ($oldVersion.Major, $oldVersion.Minor, $oldVersion.Build, $buildNumber)
	Update-Version $newVersion $assemblyInfoFilePath
}

task Compile { 
	exec { msbuild /nologo /verbosity:quiet $solutionFilePath /p:Configuration=Release }
}

task RunTests -depends Compile {
	$xunitRunner = "$srcDir\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe"
	gci . -Recurse -Include *Tests.csproj, Tests.*.csproj | % {
		$project = $_.BaseName
		if(!(Test-Path $reportsDir\xUnit\$project)){
			New-Item $reportsDir\xUnit\$project -Type Directory
		}
        .$xunitRunner "$srcDir\$project\bin\Release\$project.dll" /html "$reportsDir\xUnit\$project\index.html"
    }
}

task ILMerge -depends Compile {
	New-Item $buildOutputDir -Type Directory -ErrorAction SilentlyContinue
	$dllDir = "$srcDir\Cedar\bin\Release"
	$inputDlls = "$dllDir\Cedar.dll"
	@("Microsoft.Owin", "NewtonSoft.Json", "Owin", "System.Reactive.Core", "System.Reactive.Interfaces", "System.Reactive.Linq",`
		"System.Reactive.PlatformServices") |% { $inputDlls = "$inputDlls $dllDir\$_.dll" }
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize /allowDup /target:library /log /out:$buildOutputDir\Cedar.dll $inputDlls"
	
	$dllDir = "$srcDir\Cedar.Client\bin\Release"
	$inputDlls = "$dllDir\Cedar.Client.dll "
	@("Newtonsoft.Json") |% { $inputDlls = "$input_dlls $dllDir\$_.dll" }
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize /allowDup /target:library /log /out:$buildOutputDir\Cedar.Client.dll $inputDlls"

	$dllDir = "$srcDir\Cedar.Testing\bin\Release"
	$inputDlls = "$dllDir\Cedar.Testing.dll "
	@("Microsoft.Owin", "NewtonSoft.Json", "Inflector", "Owin", "OwinHttpMessageHandler", "System.Reactive.Core", "System.Reactive.Interfaces", 
		"System.Reactive.Linq") |% { $inputDlls = "$inputDlls $dllDir\$_.dll" }
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize /allowDup /target:library /log /out:$buildOutputDir\Cedar.Testing.dll $inputDlls"

	$dllDir = "$srcDir\Cedar.Testing.TestRunner\bin\Release"
	$inputDlls = "$dllDir\Cedar.Testing.TestRunner.exe "
	@("Microsoft.Owin", "PowerArgs", "NewtonSoft.Json", "Inflector", "Owin", "OwinHttpMessageHandler", "System.Reactive.Core", "System.Reactive.Interfaces", 
		"System.Reactive.Linq") |% { $inputDlls = "$inputDlls $dllDir\$_.dll" }
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize /allowDup /target:exe /log /out:$buildOutputDir\Cedar.Testing.TestRunner.exe $inputDlls"
}

task CreateNuGetPackages -depends ILMerge {
	$versionString = Get-Version $assemblyInfoFilePath
	$version = New-Object Version $versionString
	$packageVersion = $version.Major.ToString() + "." + $version.Minor.ToString() + "." + $version.Build.ToString() + "-build" + $buildNumber.ToString().PadLeft(5,'0')
	$packageVersion
	gci $srcDir -Recurse -Include *.nuspec | % {
		exec { .$srcDir\.nuget\nuget.exe pack $_ -o $buildOutputDir -version $packageVersion }
	}
}