<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PowerShellOnlineSigningService.Default" %>
<asp:Content ID="content1" ContentPlaceHolderID="cphHead" runat="server" >

</asp:Content>

<asp:Content ID="content2" ContentPlaceHolderID="cphBody" runat="server">
    <div class="summary master_link">
        <p>
            Download a digitally signed PowerShell scripts directly from Github.  
            Choose from either the default PowerShell Repository or search for a specific users Repository.
        </p>
        <table>
            <tr>
                <td>
                    <a href="User.aspx"><asp:Image ID="imgGitHub" runat="server" ImageUrl="~/images/githubrobot.png" /></a>
                </td>
                <td>
                    <span class="menuItem"><a href="User.aspx">Default PowerShell Repository</a></span>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Image ID="imgSearch" runat="server" ImageUrl="~/images/searchman.jpg" />
                </td>
                <td>
                    <asp:TextBox ID="tbSearch" runat="server" placeholder='Tell me who you are looking for...' 
                        ToolTip="Search for login name, username, or email address" /> &nbsp
                    <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
                </td>
            </tr>
        </table>
    </div> <%-- summary --%>
</asp:Content>

<asp:Content ID="content3" ContentPlaceHolderID="cphFooter" runat="server" />