call "%SecretsFolder%\SiteScanner\deployVars.bat"
xcopy "%SecretsFolder%\SiteScanner\LinuxScannerConfig\appsettings.json" "C:\Projects\SiteScanner\ScannerBot\bin\Release\netcoreapp3.1\publish_linux\" /y
"%tools%\azcopy" copy "C:\Projects\SiteScanner\ScannerBot\bin\Release\netcoreapp3.1\publish_linux\*" "%DeployFolder%/linux%DeploySAS%" --recursive=true
