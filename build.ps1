try {
    $c = $true
    $s = $true
    if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
        throw "未安装.NET SDK"
    }

    $publishDirectory = "Publish"
    if  (Test-Path $publishDirectory -PathType Container) {
        Remove-Item -r $publishDirectory 
    }

    New-Item -Path $publishDirectory -ItemType Directory | Out-Null
    
    Clear-Host

    $moduleDirectories = Get-ChildItem -Path . -Directory -Filter "ArchiveMaster.Module.*"

    Write-Output "正在发布win-x64"
    
    foreach ($module in $moduleDirectories) {
        dotnet publish $module.FullName -r win-x64 -c Release -o Publish/win-x64/temp
    }
    dotnet publish ArchiveMaster.UI.Desktop -r win-x64 -c Release -o Publish/win-x64 --self-contained $c /p:PublishSingleFile=$s 
    Copy-Item Publish/win-x64/temp/ArchiveMaster.Module.* Publish/win-x64
    Remove-Item -r Publish/win-x64/temp

    Write-Output "正在发布linux-x64"
        
    foreach ($module in $moduleDirectories) {
        dotnet publish $module.FullName -r linux-x64 -c Release -o Publish/linux-x64/temp
    }
    dotnet publish ArchiveMaster.UI.Desktop -r linux-x64 -c Release -o Publish/linux-x64 --self-contained $c /p:PublishSingleFile=$s
    Copy-Item Publish/linux-x64/temp/ArchiveMaster.Module.* Publish/linux-x64
    Remove-Item -r Publish/linux-x64/temp
    Remove-Item -r ./*/obj/
   
    Write-Output "操作完成"

    Invoke-Item Publish
    pause
}
catch {
    Write-Error $_
}