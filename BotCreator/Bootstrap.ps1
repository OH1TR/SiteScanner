param(
    [string]
    $username,

    [string]
    $password
)

$connectTestResult = Test-NetConnection -ComputerName sitescanner.file.core.windows.net -Port 445
if ($connectTestResult.TcpTestSucceeded) {
    cmd.exe /C "cmdkey /add:`"sitescanner.file.core.windows.net`" /user:`"localhost\sitescanner`" /pass:`"`""
    New-PSDrive -Name I -PSProvider FileSystem -Root "\\sitescanner.file.core.windows.net\install" -Persist
} else {
    Write-Error -Message "Unable to reach the Azure storage account via port 445. Check to make sure your organization or ISP is not blocking port 445, or use Azure P2S VPN, Azure S2S VPN, or Express Route to tunnel SMB traffic over a different port."
}

Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Unrestricted

cmd.exe /c "i:\installWindows.bat $username $password"
