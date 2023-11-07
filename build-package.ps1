param
(
	[Parameter(Mandatory = $true)]
	[string]$ProjectDir,

	[Parameter(Mandatory = $true)]
	[string]$PublishDir,

	[Parameter(Mandatory = $true)]
	[string]$SolutionDir,

	[Parameter(Mandatory = $true)]
	[string]$AssemblyName,

	[Parameter(Mandatory = $true)]
	[string]$Product,

	[Parameter(Mandatory = $true)]
	[string]$RuntimeIdentifier,

	[Parameter(Mandatory = $true)]
	[string]$Version
)

$binPath = Join-Path $SolutionDir 'bin'
$objPath = Join-Path $SolutionDir 'obj'
$publishPath = Join-Path $ProjectDir $PublishDir
$msiPath = Join-Path $binPath "$Product-$RuntimeIdentifier-v$Version.msi"
$wxsPath = Join-Path $objPath 'package.wxs'
$zipPath = [System.IO.Path]::ChangeExtension($msiPath, '.zip')

$Version = $Version.Substring(0, $Version.LastIndexOf('.'))

if (!(Test-Path $binPath))
{
	New-Item $binPath -ItemType Directory | Out-Null
}

if (!(Test-Path $objPath))
{
	New-Item $objPath -ItemType Directory | Out-Null
}

$wxs = [xml]"<Wix xmlns=`"http://wixtoolset.org/schemas/v4/wxs`">
<Package Name=`"$Product`" Manufacturer=`"Richard Robertson`" Version=`"$Version`" UpgradeCode=`"34486a8a-8c84-4f38-8778-4f9a84a0c263`">
	<MajorUpgrade DowngradeErrorMessage=`"A newer version of [ProductName] is already installed.`" />
	<MediaTemplate EmbedCab=`"yes`" />
	<StandardDirectory Id=`"ProgramFiles6432Folder`">
		<Directory Id=`"INSTALLFOLDER`" Name=`"!(bind.Property.ProductName)`" />
	</StandardDirectory>
	<StandardDirectory Id=`"ProgramMenuFolder`" />
	<ComponentGroup Id=`"AppComponents`" Directory=`"INSTALLFOLDER`" />
	<Feature Id=`"Main`">
		<ComponentGroupRef Id=`"AppComponents`" />
	</Feature>
</Package>
</Wix>"

$publishFiles = Get-ChildItem $publishPath

foreach ($file in $publishFiles)
{
	$component = $wxs.CreateElement('Component', 'http://wixtoolset.org/schemas/v4/wxs')
	$wxs['Wix']['Package']['ComponentGroup'].AppendChild($component) | Out-Null
	$fileElement = $wxs.CreateElement('File', 'http://wixtoolset.org/schemas/v4/wxs')
	$component.AppendChild($fileElement) | Out-Null
	$fileElement.SetAttribute('Source', $file.FullName)
	if ($file.Extension -eq '.exe' -and $file.BaseName -eq $AssemblyName)
	{
		$shortcut = $wxs.CreateElement('Shortcut', 'http://wixtoolset.org/schemas/v4/wxs')
		$fileElement.AppendChild($shortcut) | Out-Null
		$shortcut.SetAttribute('Name', $AssemblyName)
		$shortcut.SetAttribute('Directory', 'ProgramMenuFolder')
	}
}

$publishFiles | Compress-Archive -DestinationPath $zipPath

$wxs.Save($wxsPath)

wix build -arch x64 -out $msiPath $wxsPath
