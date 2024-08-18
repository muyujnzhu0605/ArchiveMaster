try {
    $c = $true
    $s = $true
    if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
        throw "未安装.NET SDK"
    }

    $publishDirectory = Read-Host "请输入发布目录（默认为Publish）"

    if ([string]::IsNullOrEmpty($publishDirectory)) {
        $publishDirectory = "Publish"
    }
    else{
        $publishDirectory.Trim('"')
    }

    if  (Test-Path $publishDirectory -PathType Container) {
        Remove-Item -r $publishDirectory 
    }

    New-Item -Path $publishDirectory -ItemType Directory | Out-Null

    Set-Location ..
    
    Clear-Host

    Write-Output "正在发布win-x64"
    dotnet publish ArchiveMaster.UI.Desktop -r win-x64 -c Release -o "$publishDirectory/win-x64" --self-contained $c /p:PublishSingleFile=$s

    Write-Output "正在发布linux-x64"
    dotnet publish ArchiveMaster.UI.Desktop -r linux-x64 -c Release -o "$publishDirectory/linux-x64" --self-contained $c /p:PublishSingleFile=$s

    Write-Output "正在发布macos-x64"
    dotnet publish ArchiveMaster.UI.Desktop -r osx-x64 -c Release -o "$publishDirectory/macos-x64" --self-contained $c /p:PublishSingleFile=$s
   
    Write-Output "操作完成"

    Invoke-Item $publishDirectory
    pause
}
catch {
    Write-Error $_
}