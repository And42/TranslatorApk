function Combine($first, $second)
{
	Return [System.IO.Path]::Combine($first, $second)
}

function GetFileHash($file)
{
	Return (Get-FileHash $file).Hash
}

function CopyFile($source, $dist)
{
	# Write-Host ("source: " + $source + "\ntarget: " + $dist) -ForegroundColor Cyan
	[System.IO.File]::Copy($source, $dist, $TRUE)
}

function GetVersion($file)
{
	Return [System.Diagnostics.FileVersionInfo]::GetVersionInfo($file).FileVersion
}

function IsNewer($oldFile, $newFile)
{
	$oldParts = $oldFile.Split(".")
	$newParts = $newFile.Split(".")
	
	For ($i = 0; $i -lt 4; $i++)
	{
		$oldPart = $oldParts[$i]
		$newPart = $newParts[$i]
	
		if ($newPart -gt $oldPart) {
			Return $true
		}
		
		if ($newPart -lt $oldPart) {
			Return $false
		}
	}
	
	Return $false
}

function CopyAndCheckFile($file, $checkVersion = $true)
{
	$fileName = [System.IO.Path]::GetFileName($file)
	$newFile = Combine $PSScriptRoot $fileName
	
	# Write-Host ("CopyAndCheckFile: " + $file) -ForegroundColor Cyan
	# Write-Host ("CopyAndCheckFile (newFile): " + $newFile) -ForegroundColor Cyan
	
	if (Test-Path $file)
	{
		if ( $checkVersion -and (Test-Path $newFile) )
		{
			# Write-Host ("SourceHash: " + (GetFileHash $file)) -ForegroundColor Cyan
			# Write-Host ("TargetHash: " + (GetFileHash $newFile)) -ForegroundColor Cyan
		
			$newVersion = GetVersion $file
			$oldVersion = GetVersion $newFile
		
			if ( ( (GetFileHash $file) -eq (GetFileHash $newFile) ) -or ( -not (IsNewer $oldVersion $newVersion) ) )
			{
				Write-Host "skipped" -ForegroundColor Gray
				Return $false
			}

			CopyFile $file $newFile
			Write-Host "updated" -ForegroundColor Cyan
			Return $true
		}

		CopyFile $file $newFile
		Write-Host "copied" -ForegroundColor Green
		Return $true
	}
	
	Write-Host "not found" -ForegroundColor Red
	Return $false
}

function CopyDependency ($dependencyLib)
{
	$fileName = [System.IO.Path]::GetFileName($dependencyLib)
	
	$pdbFile = [System.IO.Path]::ChangeExtension($dependencyLib, ".pdb")
	$pdbFileName = [System.IO.Path]::GetFileName($pdbFile)
	
	$docFile = [System.IO.Path]::ChangeExtension($dependencyLib, ".xml")
	$docFileName = [System.IO.Path]::GetFileName($docFile)
	
	Write-Host ("  ...\" + $fileName + " ") -NoNewLine
	$copied = CopyAndCheckFile $dependencyLib
	
	if ($copied) {
		Write-Host ("  ...\" + $pdbFileName + " ") -NoNewLine
		$tmp = CopyAndCheckFile $pdbFile $false
		
		Write-Host ("  ...\" + $docFileName + " ") -NoNewLine
		$tmp = CopyAndCheckFile $docFile $false
	}
}

function ProcessLibrary ($libraryPath, $includeDependencies)
{
	$fileName = [System.IO.Path]::GetFileName($libraryPath)
	$newName = Combine $PSScriptRoot $fileName
	
	$pdbFile = [System.IO.Path]::ChangeExtension($libraryPath, ".pdb")
	$docFile = [System.IO.Path]::ChangeExtension($libraryPath, ".xml")
	
	Write-Host ($libraryPath + " ") -NoNewLine
	$copied = CopyAndCheckFile $libraryPath
	
	if ($copied) {
		Write-Host ($pdbFile + " ") -NoNewLine
		$tmp = CopyAndCheckFile $pdbFile $false
		
		Write-Host ($docFile + " ") -NoNewLine
		$tmp = CopyAndCheckFile $docFile $false
	}
	
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