<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="PowerShellOnlineSigningService.Search" %>
<asp:Content ID="cphHead" ContentPlaceHolderID="cphHead" runat="server" />

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
    <div class="search_wrapper">
        <asp:TextBox ID="tbSearch" runat="server" placeholder='Tell me who you are looking for...' ToolTip="Enter a search criteria of the name of a GitHub User (only top 10 results are shown)" Wrap="False" OnTextChanged="btnSearch_Click" />
        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
    </div>
    <asp:PlaceHolder ID="phMessage" runat="server" />
    <asp:PlaceHolder ID="phResults" runat="server" />
    <br style="clear: left" />
</asp:Content>

<asp:Content ID="cphFooter" ContentPlaceHolderID="cphFooter" runat="server"/>

