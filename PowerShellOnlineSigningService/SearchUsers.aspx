<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="SearchUsers.aspx.cs" Inherits="PowerShellOnlineSigningService.SearchUsers" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="cphHead" runat="server" />

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
    <div class="search_wrapper">
        <asp:TextBox ID="tbSearch" runat="server" ToolTip="Enter a search criteria of the name of a GitHub User (only top 10 results are shown)" Wrap="False" OnTextChanged="btnSearch_Click" />
        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
    </div>
    <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" CssClass="gvUsers" ShowHeader="false"
        CellPadding="3" EnableModelValidation="True" GridLines="None" OnRowDataBound="gvUsers_RowDataBound" >

        <Columns>

            <asp:TemplateField ItemStyle-CssClass="master_image">
                <ItemTemplate><asp:Image runat="server" ID="avatarURL" /></ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField ItemStyle-CssClass="gvUsers_loginName master_link">
                <ItemTemplate><asp:HyperLink ID="contentLink" runat="server" /></ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField Visible="false">
                <ItemTemplate><asp:Label ID="Login" runat="server" /></ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField Visible="false">
                <ItemTemplate><asp:Label ID="userURL" runat="server" /></ItemTemplate>
            </asp:TemplateField>

        </Columns>

    </asp:GridView>
</asp:Content>

<asp:Content ID="cphFooter" ContentPlaceHolderID="cphFooter" runat="server"/>
