$rawDir = Get-Location
Set-Location ..
dotnet publish ArchiveMaster.UI.Android -f net8.0-android -r android-arm64 -c Release -p:AndroidKeyStore=true  
Invoke-Item C:\Users\$env:USERNAME\AppData\Local\Temp\Release\ArchiveMaster.UI.Android\net8.0-android\android-arm64\publish
Set-Location $rawDir
pause