While trying to simplify the digital signing of PowerShell scripts I decided to 
write a web based interface to the PowerShell commands.  The website has a file
upload option that will accept only .ps1 files and then provides a link to
download the signed PowerShell script.

Accessing the website should require user authenication.  When submitting a
script to be signed the authenticated user (script author) information will be 
inserted into the top of the script file before completing the signature.

If a previously edited PowerShell script is uploaded the website will replace
any previous author information as well as remove previous signature 
information it finds on the script.

A digital signing certificate must already be installed on the webserver and
located in the LocalMachine\My certificate folder.  The website requires 
.NET 4.5 and can be run using a web service account that has been granted 
access to a digital signing certificate in the local machine store.

I would be interested to hear from anyone who is able to sign PowerShell 
scripts via C# so I can replace the shell out code. 
