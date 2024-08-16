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

    if  (Test-Path $publishDirectory -PathType Container) {
        Remove-Item -r $publishDirectory 
    }

    New-Item -Path $publishDirectory -ItemType Directory | Out-Null
    
    Clear-Host

    $moduleDirectories = Get-ChildItem -Path . -Directory -Filter "ArchiveMaster.Module.*"

    Write-Output "正在发布win-x64"
    
    foreach ($module in $moduleDirectories) {
        dotnet publish $module.FullName -r win-x64 -c Release -o "$publishDirectory/win-x64"
    }
    dotnet publish ArchiveMaster.UI.Desktop -r win-x64 -c Release -o "$publishDirectory/win-x64" --self-contained $c 

    Write-Output "正在发布linux-x64"
        
    foreach ($module in $moduleDirectories) {
        dotnet publish $module.FullName -r linux-x64 -c Release -o "$publishDirectory/linux-x64"
    }
    dotnet publish ArchiveMaster.UI.Desktop -r linux-x64 -c Release -o "$publishDirectory/linux-x64" --self-contained $c

    Write-Output "正在发布macos-x64"
        
    foreach ($module in $moduleDirectories) {
        dotnet publish $module.FullName -r osx-x64 -c Release -o "$publishDirectory/macos-x64"
    }
    dotnet publish ArchiveMaster.UI.Desktop -r osx-x64 -c Release -o "$publishDirectory/macos-x64" --self-contained $c
   
    Write-Output "操作完成"

    Invoke-Item Publish
    pause
}
catch {
    Write-Error $_
}