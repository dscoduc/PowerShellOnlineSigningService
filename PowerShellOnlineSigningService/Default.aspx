<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="UserContent.aspx.cs" Inherits="PowerShellOnlineSigningService.UserContent" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="cphHead" runat="server" />

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">

    <div>
        <ul>
            <li id="default">
                <asp:HyperLink ID="default_link" runat="server" NavigateUrl="~/UserContent.aspx?owner=dscoduc">Default GitHub Repository</asp:HyperLink>
                <p>This link will take you to the default repository holding various PowerShell scripts.</p>
            </li>
            <li id="search">
                <asp:HyperLink ID="search_link" runat="server" NavigateUrl="~/SearchUsers.aspx">Search for a GitHub user</asp:HyperLink>
                <p>The search page will allow you to lookup a user and then view their GitHub Repositories</p>
            </li>
        </ul>
    </div>

</asp:Content>

<asp:Content ID="cphFooter" ContentPlaceHolderID="cphFooter" runat="server"/>
