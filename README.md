# PowerShell Online Signing Service

While trying to simplify the digital signing of PowerShell scripts I decided to 
write a web based interface to the PowerShell commands that ties into a GitHub
Repository.  The website will accept display .ps1 files by default and then 
provides a link to download the script after being signed by PowerShell.

Accessing the website should require user authenication to protect the integrity
of your signed scripts.  If a previously signed PowerShell script downloaded the 
website will replace any previous author information as well as remove previous 
signature information it finds in the script.

A digital signing certificate must already be installed on the web server and
located in the LocalMachine\My certificate folder.  The website requires .NET 4.5 
and can be run using a web service account that has been granted access to a 
digital signing certificate in the local machine store.

I would be interested to hear from anyone who is able to sign PowerShell 
scripts natively in C# so I can replace the shell out to PowerShell code. 

Install Steps:

Create new folder for root of Web Application 
	Ex. c:\InetPub\PowerShellSigning)
Copy Web Application files into new Web Applicaton folder
Create Application Pool 
	Ex. Name=PowerShell CLR=v4 Pipeline=Integrated
Create new Website using new AppPool
	Ex. PowerShell Signing
Configure Web Site Authentication to only accept Windows Integrated
Delegate Modify rights to App_Data folder to IIS AppPool account 
	Ex. icacls c:\inetpub\PowerShellSigning\App_Data /grant "IIS APPPOOL\PowerShell":(OI)(CI)(F)
Install Code Signing Certificate into Local Machine Certificate store
Delegate read access to Code Signing Certificate private key

![Main screen snapshot](https://raw.githubusercontent.com/dscoduc/PowerShellOnlineSigningService/master/PowerShellOnlineSigningService.PNG)