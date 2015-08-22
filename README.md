# PowerShell Online Signing Service

While trying to simplify the digital signing of PowerShell scripts I decided to 
write a web based interface to the PowerShell commands that ties into a GitHub
Repository.  The goal is to allow you to download a PowerShell script that you
have stored in GitHub and have that script digitally signed.

The project is configured with a default owner and repository option which will
allow the user to quickly go to a common store while still having the option
of locating a different owners repository.

Once a repository is selected the page will display a list of allowed files,
by default only .ps1 files.  When a file is selected that file will be downloaded
from GitHub to the web server, digitally signed, and then served to the user for
download into their machine.

Accessing the website should require user authenication to protect the integrity
of your signed scripts.  If the PowerShell script in GitHub is already digitally
signed the website will replace the previous author information and signature
before adding the new user info and signature.

A code signing certificate must already be installed on the web server and
located in the LocalMachine\My certificate folder.  The code signing certificate 
must also be configured to allow the AppPool service account to access the 
private key.

The website has been built using .NET 4.5 and ASP.NET web services.

I would be interested to hear from anyone who is knowledgable in signing
PowerShell scripts natively in C# so I can replace the shell out syntax with
something native to C#.

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
Grant read access to private key on Code Signing Certificate to App Pool account
	Ex. username = "App Pool\PowerShell"

![Main screen snapshot](https://raw.githubusercontent.com/dscoduc/PowerShellOnlineSigningService/master/PowerShellOnlineSigningService.PNG)
