<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="SearchUsers.aspx.cs" Inherits="PowerShellOnlineSigningService.SearchUsers" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="cphHead" runat="server" >

</asp:Content>

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
    <div class="search_wrapper">
        <p>Enter a search string and I will help you find a GitHub Repository...</p>
        <asp:TextBox ID="tbSearch" runat="server" ToolTip="Enter a search criteria of the name of a GitHub User (only top 10 results are shown)" Wrap="False" OnTextChanged="btnSearch_Click" />
        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
    </div>

    <asp:DataList ID="dtUsers" runat="server" RepeatColumns="4" CellPadding="5" CssClass="dlUsers master_link" OnItemDataBound="dtUsers_DataBound">
        <ItemTemplate>
            <span style="text-align: center">
                <asp:Image ID="avatar" runat="server" />
                <br />
                <asp:HyperLink ID="avatarLink" runat="server"/>
            </span>
        </ItemTemplate>
    </asp:DataList>

</asp:Content>

<asp:Content ID="cphFooter" ContentPlaceHolderID="cphFooter" runat="server"/>
