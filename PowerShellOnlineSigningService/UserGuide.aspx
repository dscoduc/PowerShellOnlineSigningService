<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="UserGuide.aspx.cs" Inherits="PowerShellOnlineSigningService.UserGuide" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphHead" runat="server" >
    <script type="text/jscript" src="//code.jquery.com/jquery-1.10.2.js"></script>
    <script type="text/javascript" src="//code.jquery.com/ui/1.11.4/jquery-ui.js"></script>
    <script type="text/javascript">
          $(function () {
              $("#overview").accordion();
              $("#faq").accordion();
          });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cphBody" runat="server">
    <div id="overview" class="accordion">
        <h3>What is the PowerShell Online Signing Portal?</h3>
        <div>
            <p>
                The PowerShell Online Signing Portal provides you the ability to download digitally signed PowerShell scripts without having to know anything about
                signing and without having to obtain a digital signature.
            </p>
            <p>
                Working with Rackspace Enterprise GitHub the portal will retrieve your PowerShell script, digitally sign it using a Rackspace.corp signing 
                certificate, and send the signed script to you for immediate use.
            </p>
            <p>
                In order to use this portal you will need to have registered an account in GitHub and your script will need to be uploaded into a public repository.  
            More information about using and creating a GitHub account can be located at <a href="https://github.rackspace.com" target="_blank">https://github.rackspace.com</a>.
            </p>
        </div>
        <h3>Home Page</h3>
        <div>
            <p>
                Enter the address <a href="https://powersign.rackspace.corp">https://powersign.rackspace.corp</a> and log into the portal with your 
                Rackspace SSO credentials.  You will be presented with the welcome screen.
            </p>
            <p>
                <asp:Image ID="imgDefault" runat="server" ImageUrl="~/images/Default.jpg" />
            </p>
            <p>
                The portal home page is configured with a link to a default portal where the GET-IAM team has stored PowerShell useful PowerShell scripts.  
                Clicking on the “Default PowerShell Repository” link will take you to the GET-IAM repository.
            </p>
            <p>
                The home page of the portal also contains an input box and search button that allows you to perform a search for a users GitHub account.
            </p>
            <p>
                To access your own public repository enter your Login ID name into the input box.  Locating another users repository is done by entering either 
                a Rackspace SSO Login ID, First Name, Last Name, or First and Last Name.  
            </p>
            <p>
                For example, to access Chris Blankenship’s portal you could enter chri8859, Chris, Blankenship, or Chris Blankenship into the input box.
            </p>
            <p>
                When you have entered in your search criteria click on the Search button.
            </p>
            <p>
                <strong>NOTE:</strong><br />
                The portal will return a list of user accounts that match the search criteria entered in the home page input box.  If there is only a single match to 
                your search criteria you will automatically be shown a list of public repositories available for the matching user account.
            </p>
            <p>
                If you do not see the user account you were looking for then it is possible the criteria you used did not match (ex. First Name or Last Name 
                was not populated in GitHub by the user) or the number of results were greater that what was allowed to be displayed – try looking for a specific 
                Login ID such as Chri8859 or use a more specific search criteria such as Chris instead of Chr.
            </p>
        </div>
        <h3>Search Results</h3>
        <div>
            <p>
                Once you have clicked on the Search button a list of matching Rackspace SSO Login ID’s will be displayed in the portal search page.
            </p>
            <p>
                <asp:Image ID="imgSearch" runat="server" ImageUrl="~/images/Search.jpg" />
            </p>
            <p>
                Locate and click on the user account you wish to access.  If you do not see the results you are expecting then enter a new search criteria in 
                the input box and click the Search button.
            </p>
        </div>
        <h3>User Repositories</h3>
        <div>
            <p>
                Once you have clicked on a user account in the search results you will be presented with a list of public repositories for that user account.
                Only public repositories will be displayed in the results.
            </p>
            <p>
                <asp:Image ID="imgUserRepositories" runat="server" ImageUrl="~/images/User_Repositories.jpg" />
            </p>
            <p>
                Locate and click on the name of the repository that contains the scripts you are wanting to access.
            </p>
        </div>
        <h3>User Scripts</h3>
        <div>
            <p>
                Once you have clicked on a repository a list of PowerShell scripts will be displayed.
            </p>
            <p>
                <asp:Image ID="imgUserScripts" runat="server" ImageUrl="~/images/User_Scripts.jpg" />
            </p>
            <p>
                Click on the script you wish to download and your browser will download the script already signed by the portal.
            </p>                    
        </div>
        <h3>Frequently Asked Questions</h3>
        <div>
            <p>
                Once you have clicked on a repository a list of PowerShell scripts will be displayed.
            </p>
            <p>
                <asp:Image ID="Image1" runat="server" ImageUrl="~/images/User_Scripts.jpg" />
            </p>
            <p>
                Click on the script you wish to download and your browser will download the script already signed by the portal.
            </p>                    
        </div>
    </div>

    <div id="faq" class="accordion">
        <h3>Question One</h3>
        <div>
            <p>
                Something here
            </p>
        </div>
        <h3>Question two</h3>
        <div>
            <p>
                Something here
            </p>
        </div>
        <h3>Question three</h3>
        <div>
            <p>
                Something here
            </p>
        </div>

    </div>


</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="cphFooter" runat="server" />
