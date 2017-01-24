param(
    $outputDirectory = (property outputDirectro "artifacts"),
	$configuration = (property configuration "Release")
)
$projects = Get-SolutionProjects
$absoluteOutputDirectory= "$((Get-Location).Path)\$outputDirectory"

task Init {
	if((Test-Path $absoluteOutputDirectory))
	{
		Write-Host "Cleaning artifacts directory $absoluteOutputDirectory"
		Remove-Item "$absoluteOutputDirectory" -Recurse -Force -ErrorAction SilentlyContinue | Out-Null		
	}
	New-Item $absoluteOutputDirectory -ItemType Directory | Out-Null

	$projects | 
		ForEach-Object {
		Write-Host "Cleaning bin and obj folders for $($_.Directory)"
		Remove-Item "$($_.Directory)\bin" -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
		Remove-Item "$($_.Directory)\obj" -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
	}
}

task Compile{
	use "14.0" MSBuild
	$projects |
		ForEach-Object {
			if($_.IsWebProject)
			{
				$webOutDir = "$absoluteOutputDirectory\$($_.Name)"
				$outDir = "$absoluteOutputDirectory\$($_.Name)\bin"	
			
				Write-Host "Compiling $($_.Name) to $webOutDir"

				exec { MSBuild $($_.Path) /p:Configuration=$configuration /nologo /p:DebugType=None /p:Platform=AnyCpu /p:OutDir=$outDir /verbosity:quiet /p:WebProjectOutputDir=$webOutDir }
			} else
			{
				$outDir = "$absoluteOutputDirectory\$($_.Name)"	
				Write-Host "Compiling $($_.Name) to $outDir"

				exec { MSBuild $($_.Path) /p:Configuration=$configuration /nologo /p:DebugType=None /p:Platform=AnyCpu /p:OutDir=$outDir /verbosity:quiet }
			}	
		}
}

task Test {
	$projects |
		ForEach-Object {
			$xunitPath = Get-PackagePath "xunit.runner.console" $($_.Directory)
			if($xunitPath -eq $null){
				return
			} 
			$xunitRunner = "$xunitPath\tools\xunit.console.exe"
			exec { & $xunitRunner $absoluteOutputDirectory\$($_.Name)\$($_.Name).dll -xml "$absoluteOutputDirectory\xunit_$($_.Name).xml" -html "$absoluteOutputDirectory\xunit_$($_.Name).html" -nologo }
		}
}

task Pack {
	$projects |
		ForEach-Object {
			$octopusToolsPath = Get-PackagePath "OctopusTools" $($_.Directory)
			if($octopusToolsPath -eq $null){
				return
			} 

			$version = Get-Version $_.Directory
			Write-Host "Packing $($_.Name) $version to $absoluteOutputDirectory"
			exec { & $octopusToolsPath\tools\Octo.exe pack --basePath=$absoluteOutputDirectory\$($_.Name) --outFolder=$absoluteOutputDirectory --id=$($_.Name) --version=$version --overwrite}
		}
}

task Push {
	$projects |
		ForEach-Object {
			$octopusToolsPath = Get-PackagePath "OctopusTools" $($_.Directory)
			if($octopusToolsPath -eq $null){
				return
			} 

			$package = @(Get-ChildItem $absoluteOutputDirectory\$($_.Name)*.nupkg)
			exec { NuGet push $package -Source "$env:octopusDeployServer/nuget/packages" -ApiKey $env:octopusDeployApiKey }
		}
}

task Release {
	$projects |
		ForEach-Object {
			$octopusToolsPath = Get-PackagePath "OctopusTools" $($_.Directory)
			if($octopusToolsPath -eq $null){
				return
			} 
			$version = Get-Version $_.Directory
			Write-Host "Creating release $version of $($_.Name) on $env:octopusDeployServer"
			exec { & $octopusToolsPath\tools\Octo.exe create-release --server="$env:octopusDeployServer" --apiKey="$env:octopusDeployApiKey" --project="$($_.Name)" --version="$version" --packageVersion="$version" --ignoreexisting }
		}
}

task Deploy{
	$projects |
		ForEach-Object {
			$octopusToolsPath = Get-PackagePath "OctopusTools" $($_.Directory)
			if($octopusToolsPath -eq $null){
				return
			} 
			$version = Get-Version $_.Directory
			Write-Host "Deploying release $version of $($_.Name) on $env:_octopusDeployServer to $env:environment"
			exec { & $octopusToolsPath\tools\Octo.exe deploy-release --server="$env:octopusDeployServer" --apiKey="$env:octopusDeployApiKey" --project="$($_.Name)" --version="$version" --deployto="$env:environment" }
		}
}


task dev Init, Compile, Test
task . dev

task ci dev, Pack, Push, Release, Deploy