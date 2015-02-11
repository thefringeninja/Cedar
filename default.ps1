properties {
	$projectName = "Cedar"
	$buildNumber = 0
	$rootDir  = Resolve-Path .\
	$buildOutputDir = "$rootDir\build"
	$mergedDir = "$buildOutputDir\merged"
	$reportsDir = "$buildOutputDir\reports"
	$srcDir = "$rootDir\src"
	$solutionFilePath = "$srcDir\$projectName.sln"
	$assemblyInfoFilePath = "$srcDir\SharedAssemblyInfo.cs"
	$ilmerge_path = "$srcDir\packages\ILMerge.2.14.1208\tools\ilmerge.exe"
}

task default -depends Clean, UpdateVersion, RunTests, CreateNuGetPackages

task Clean {
	Remove-Item $buildOutputDir -Force -Recurse -ErrorAction SilentlyContinue
	exec { msbuild /nologo /verbosity:quiet $solutionFilePath /t:Clean /p:platform="Any CPU"}
}

task UpdateVersion {
	$version = Get-Version $assemblyInfoFilePath
	$oldVersion = New-Object Version $version
	$newVersion = New-Object Version ($oldVersion.Major, $oldVersion.Minor, $oldVersion.Build, $buildNumber)
	Update-Version $newVersion $assemblyInfoFilePath
}

task Compile {
	exec { msbuild /nologo /verbosity:quiet $solutionFilePath /p:Configuration=Release /p:platform="Any CPU"}
}

task RunTests -depends Compile {
	$xunitRunner = "$srcDir\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe"

	.$xunitRunner "$srcDir\Cedar.Tests\bin\Release\Cedar.Tests.dll" /html "$reportsDir\xUnit\$project\index.html"
}

task ILMerge -depends Compile {
	New-Item $mergedDir -Type Directory -ErrorAction SilentlyContinue

	$dllDir = "$srcDir\Cedar\bin\Release"
	$inputDlls = "$dllDir\Cedar.dll"
	@(	"CuttingEdge.Conditions",
		"System.Reactive.Core",
		"System.Reactive.Interfaces",
		"System.Reactive.Linq",
		"System.Reactive.PlatformServices") |% { $inputDlls = "$inputDlls $dllDir\$_.dll" }
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize /allowDup /target:library /log /out:$mergedDir\Cedar.dll $inputDlls"

	$dllDir = "$srcDir\Cedar.NEventStore\bin\Release"
	$inputDlls = "$dllDir\Cedar.NEventStore.dll"
	@(	"System.Reactive.Core",
		"System.Reactive.Interfaces",
		"System.Reactive.Linq",`
		"System.Reactive.PlatformServices") |% { $inputDlls = "$inputDlls $dllDir\$_.dll" }
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize /allowDup /target:library /log /out:$mergedDir\Cedar.NEventStore.dll $inputDlls"

	$dllDir = "$srcDir\Cedar.GetEventStore\bin\Release"
	$inputDlls = "$dllDir\Cedar.GetEventStore.dll"
	@(	"CuttingEdge.Conditions",
		"Newtonsoft.Json",
		"System.Reactive.Core",
		"System.Reactive.Interfaces",
		"System.Reactive.Linq",`
		"System.Reactive.PlatformServices") |% { $inputDlls = "$inputDlls $dllDir\$_.dll" }
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize /allowDup /target:library /log /out:$mergedDir\Cedar.GetEventStore.dll $inputDlls"

	$dllDir = "$srcDir\Cedar.Testing\bin\Release"
	$inputDlls = "$dllDir\Cedar.Testing.dll "
	@(	"Inflector",
		"OwinHttpMessageHandler",
		"System.Reactive.Core",
		"System.Reactive.Interfaces",
		"System.Reactive.Linq",`
		"KellermanSoftware.Compare-NET-Objects",
		"System.Reactive.PlatformServices") |% { $inputDlls = "$inputDlls $dllDir\$_.dll" }
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize /allowDup /target:library /log /out:$mergedDir\Cedar.Testing.dll $inputDlls"

	$dllDir = "$srcDir\Cedar.Testing.TestRunner\bin\Release"
	$inputDlls = "$dllDir\Cedar.Testing.TestRunner.exe "
	@("PowerArgs") |% { $inputDlls = "$inputDlls $dllDir\$_.dll" }
	Invoke-Expression "$ilmerge_path /targetplatform:v4 /internalize /allowDup /target:exe /log /out:$mergedDir\Cedar.Testing.TestRunner.exe $inputDlls"
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
