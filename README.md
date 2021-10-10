# SiteScanner
Crawls a list of domains, finds open ports(80,443,8080-8089), and screenshots them. It also saves all web traffic to HAR file.

This is a ducktape project of Firefox, Selenium, BrowserMob, and Nmap. It requires Windows and MSSQL DB.

Requirements: install these packages: amazon-corretto-8.302.08.1-windows-x64-jre, npcap-1.55, vc_redist.x86(2015) to the system. And geckodriver.exe to the folder with Scanner.exe. See Scanner.exe.config for paths. You may have to create firefox profile 'selenium' (Start->run->Firefox.exe -p). Also, install browsermob CA certs to Firefox. You can find them from browsermobs install directory. Disable OCSP. And set Firefox http and https proxy to 127.0.0.1:8081.

If you want to do something like this, please check https://www.kali.org/tools/eyewitness/.


