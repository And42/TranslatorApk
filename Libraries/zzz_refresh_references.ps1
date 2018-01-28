function Combine($first, $second)
{
	Return [System.IO.Path]::Combine($first, $second)
}

function GetFileHash ($file)
{
	Return (Get-FileHash $file).Hash
}

function CopyFile ($source, $dist)
{
	# Write-Host ("source: " + $source + "\ntarget: " + $dist) -ForegroundColor Cyan
	[System.IO.File]::Copy($source, $dist, $TRUE)
}

function CopyAndCheckFile ($file)
{
	$fileName = [System.IO.Path]::GetFileName($file)
	$newFile = Combine $PSScriptRoot $fileName
	
	# Write-Host ("CopyAndCheckFile: " + $file) -ForegroundColor Cyan
	# Write-Host ("CopyAndCheckFile (newFile): " + $newFile) -ForegroundColor Cyan
	
	if (Test-Path $file)
	{
		if (Test-Path $newFile)
		{
			# Write-Host ("SourceHash: " + (GetFileHash $file)) -ForegroundColor Cyan
			# Write-Host ("TargetHash: " + (GetFileHash $newFile)) -ForegroundColor Cyan
		
			if ((GetFileHash $file) -eq (GetFileHash $newFile))
			{
				Write-Host "skipped" -ForegroundColor Gray
			}
			else 
			{
				CopyFile $file $newFile
				Write-Host "updated" -ForegroundColor Cyan
			}
		}
		else
		{
			CopyFile $file $newFile
			Write-Host "copied" -ForegroundColor Green
		}
	}
	else 
	{
		Write-Host "not found" -ForegroundColor Red
	}
}

function CopyDependency ($dependencyLib)
{
	$fileName = [System.IO.Path]::GetFileName($dependencyLib)
	
	$pdbFile = [System.IO.Path]::ChangeExtension($dependencyLib, ".pdb")
	$pdbFileName = [System.IO.Path]::GetFileName($pdbFile)
	
	$docFile = [System.IO.Path]::ChangeExtension($dependencyLib, ".xml")
	$docFileName = [System.IO.Path]::GetFileName($docFile)
	
	Write-Host ("  ...\" + $fileName + " ") -NoNewLine
	CopyAndCheckFile $dependencyLib
	
	Write-Host ("  ...\" + $pdbFileName + " ") -NoNewLine
	CopyAndCheckFile $pdbFile
	
	Write-Host ("  ...\" + $docFileName + " ") -NoNewLine
	CopyAndCheckFile $docFile
}

function ProcessLibrary ($libraryPath, $includeDependencies)
{
	$fileName = [System.IO.Path]::GetFileName($libraryPath)
	$newName = Combine $PSScriptRoot $fileName
	
	$pdbFile = [System.IO.Path]::ChangeExtension($libraryPath, ".pdb")
	$docFile = [System.IO.Path]::ChangeExtension($libraryPath, ".xml")
	
	Write-Host ($libraryPath + " ") -NoNewLine
	CopyAndCheckFile $libraryPath
	
	Write-Host ($pdbFile + " ") -NoNewLine
	CopyAndCheckFile $pdbFile
	
	Write-Host ($docFile + " ") -NoNewLine
	CopyAndCheckFile $docFile
	
	if ($includeDependencies -eq $FALSE)
	{
		""
		return
	}
	
	if (Test-Path $libraryPath)
    {
		$folder = [System.IO.Path]::GetDirectoryName($libraryPath)
		$dependencyDlls = [System.IO.Directory]::GetFiles($folder, "*.dll")
		
		ForEach ($dependency in $dependencyDlls)
		{
			If ($dependency -ne $libraryPath)
			{
				CopyDependency $dependency
			}
		}
    }
	
	""
}

$libraries = Get-Content(Combine $PSScriptRoot "zzz_dependencies.txt")

$includeDependencies = $TRUE

ForEach ($library in $libraries)
{
	if ($library -eq "only_dll")
	{
		$includeDependencies = $FALSE
		continue
	}

    ProcessLibrary $library $includeDependencies
}