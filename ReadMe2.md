# PowerShell Online Signing
## Solution Overview
One of the security features built into Windows PowerShell scripts is the ability to define an execution policy on the server.  The execution policy allows an administrator to restrict the execution of scripts on a server through the use of four different secruity levels:
* Restricted
* AllSigned
* RemoteSigned
* Unrestricted

The **Resticted** level is the default execution policy which does not allow the execution of scripts in favor of only allowing interactive execution.  The **AllSigned** level only allows the execution of scripts and configuration files that are signed by a trusted publisher.  The **RemoteSigned** level allows the execution of scripts and configuration files downloaded from communication applications that are signed by a publisher that you trust.  Finally the **Unrestricted** level allows the execution of any scripts without requiring the script to be signed.

In an ideal world we would want to only allow signed scripts to execute however this can be difficult to accomplish if the ability to sign PowerShell scripts is not simplified.  This solution is an attempt to simplify the process.  Very often one of the first steps for an administrator is to set the execution policy to **Unrestricted**.

I believe we need to change this behavior and ensure that critical systems should be configured with the **AllSigned** level.  To accomplish this I decided to develop a web portal that can reduce the difficulties of signing scripts.  One critical component of this solution is the dependency and interaction with GitHub.

There are many benefits of storing PowerShell scripts in GitHub including the sharing of code across teammates and maintain version control for changes and updates. This solution will use GitHub as the source of scripts that are to be signed and downloaded.  This interaction with GitHUb allows the solution to not modify the original script during the signing process.
## Solution Details
### Web Site
The solution web site is built on ASP.NET 4.5 using C# running on Windows Server 2012 R2.  The core of the website uses Master Pages to render a common format across the following pages: 
* Default.aspx
* Search.aspx
* User.aspx
* DownloadFile.ashx
##### Default.aspx
The Default.aspx page is the entry point for the web site and provides two main option to the web user.  The first option is a link to the default GitHub owner as defined in the web.config file.  When the web user clicks on the first option they will automatically be redirected to the User.aspx page with the pre-defined owner name in the query string (e.g. User.aspx?o=dscoduc). 

The second option is an input field and search button which allows the web user to search for a GitHb owner.  The input field criteria can include the GitHub owner's login ID, first name, and/or last name.  Results are limited by GitHub and if a results are missing a desired owner than a more specific search criteria should be submitted. 

When the user clicks on the search button they will automatically be redirected to the Search.aspx page with the search criteria added to the query string (e.g. Search.aspx?s=dsco). 
##### Search.aspx

##### User.aspx

##### DownloadFile.ashx
### Application Pool
The web site runs under an application pool configured with a dedicated service account.  The service account has been granted permissions to the private key of a digital signing certificate that has been installed on the server.  Additional permissions have been granted to the service account which allows for read/write access to the ASP.NET App_Data folder where both temporary files and debug log files are created.
### JSON Serialization
To assist in the handling of JSON responses coming from GitHub this solution relies on the Newtonsoft Json v7.0.1 module.  This allows a quick translation from JSON to a C# object class.  
### Logging Framework
To handle the logging this solution relies on the Log4Net v2.0.3 framework.  This framework simplifies the handling of writing to both a log file and the Windows Event Log. A configuration file named log4net.config is placed in the root of the website and contains the settings required for the solution to log effectively.

## How to use
Accessing the solutions web site requires you to authenticate to the system.  This is a necessary step to ensure the integrity of the code signing certificate and prevent malicious use of the signing certificate.
