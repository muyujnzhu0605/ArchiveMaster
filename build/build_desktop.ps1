$rawDir = Get-Location

try {
    $s = $true
    if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
        throw "未安装.NET SDK"
    }

    $publishDirectory = Read-Host "请输入发布目录（默认为Publish）"

    if ([string]::IsNullOrEmpty($publishDirectory)) {
        $publishDirectory = "Publish"
    }
    else {
        $publishDirectory.Trim('"')
    }

    Set-Location $PSScriptRoot/..

    if (Test-Path $publishDirectory -PathType Container) {
        Remove-Item -r $publishDirectory 
    }

    New-Item -Path $publishDirectory -ItemType Directory | Out-Null
    
    Clear-Host

    Write-Output "正在发布win-x64"
    dotnet publish ArchiveMaster.UI.Desktop -r win-x64 -c Release -o "$publishDirectory/win-x64" /p:PublishSingleFile=$s --no-self-contained
    Move-Item "$publishDirectory/win-x64/ArchiveMaster.UI.Desktop.exe" "$publishDirectory/win-x64/ArchiveMaster.exe"

    Write-Output "正在发布linux-x64"
    dotnet publish ArchiveMaster.UI.Desktop -r linux-x64 -c Release -o "$publishDirectory/linux-x64" /p:PublishSingleFile=$s --no-self-contained
    Move-Item "$publishDirectory/linux-x64/ArchiveMaster.UI.Desktop" "$publishDirectory/linux-x64/ArchiveMaster"

    Write-Output "正在发布macos-x64"
    dotnet publish ArchiveMaster.UI.Desktop -r osx-x64 -c Release -o "$publishDirectory/macos-x64" /p:PublishSingleFile=$s --no-self-contained
    Move-Item "$publishDirectory/macos-x64/ArchiveMaster.UI.Desktop" "$publishDirectory/macos-x64/ArchiveMaster"

    
    Write-Output "正在发布win-x64-self-contained"
    dotnet publish ArchiveMaster.UI.Desktop -r win-x64 -c Release -o "$publishDirectory/win-x64-self-contained" --self-contained /p:PublishSingleFile=$s
    Move-Item "$publishDirectory/win-x64-self-contained/ArchiveMaster.UI.Desktop.exe" "$publishDirectory/win-x64-self-contained/ArchiveMaster.exe"

    Write-Output "正在发布linux-x64-self-contained"
    dotnet publish ArchiveMaster.UI.Desktop -r linux-x64 -c Release -o "$publishDirectory/linux-x64-self-contained" --self-contained /p:PublishSingleFile=$s
    Move-Item "$publishDirectory/linux-x64-self-contained/ArchiveMaster.UI.Desktop" "$publishDirectory/linux-x64-self-contained/ArchiveMaster"

    Write-Output "正在发布macos-x64-self-contained"
    dotnet publish ArchiveMaster.UI.Desktop -r osx-x64 -c Release -o "$publishDirectory/macos-x64-self-contained" --self-contained /p:PublishSingleFile=$s
    Move-Item "$publishDirectory/macos-x64-self-contained/ArchiveMaster.UI.Desktop" "$publishDirectory/macos-x64-self-contained/ArchiveMaster"
   
    Write-Output "操作完成"

    Invoke-Item $publishDirectory
}
catch {
    Write-Error $_
}

Set-Location $rawDir

pause