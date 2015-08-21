<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PowerShellOnlineSigningService.Default" %>
<asp:Content ID="content1" ContentPlaceHolderID="cphHead" runat="server" >

</asp:Content>

<asp:Content ID="content2" ContentPlaceHolderID="cphBody" runat="server">
    <div class="summary master_link">
        <p>
            This site provides the ability to download a digitally signed PowerShell script directly from
            Enterprise Github.  Choose from either the default PowerShell Repository or search for a
            specific users GitHub Repository.            
        </p>
        <table>
            <tr>
                <td>
                    <a href="UserContent.aspx"><asp:Image ID="imgGitHub" runat="server" ImageUrl="~/images/githubrobot.png" /></a>
                </td>
                <td>
                    <span class="menuItem"><a href="UserContent.aspx">Rackspace PowerShell Repository</a></span>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Image ID="imgSearch" runat="server" ImageUrl="~/images/searchman.jpg" />
                </td>
                <td>
                    <asp:TextBox ID="tbSearch" runat="server" ToolTip="Search for login name, username, or email address" /> &nbsp<asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
                </td>
            </tr>
        </table>
    </div> <%-- summary --%>
</asp:Content>

<asp:Content ID="content3" ContentPlaceHolderID="cphFooter" runat="server" />