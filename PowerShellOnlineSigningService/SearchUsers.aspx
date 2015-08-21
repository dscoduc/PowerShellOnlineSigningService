<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="SearchUsers.aspx.cs" Inherits="PowerShellOnlineSigningService.SearchUsers" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="cphHead" runat="server" />

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
    <div id="breadcrumb_wrapper" runat="server">
        <asp:TextBox ID="tbSearch" runat="server" ToolTip="Enter a search criteria of the name of a GitHub User (only top 10 results are shown)" Wrap="False" OnTextChanged="btnSearch_Click" />
        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />
    </div>
    <asp:GridView ID="gvUsers" runat="server" AllowPaging="False" AutoGenerateColumns="False" CssClass="gvUsers" PageSize="20" ShowHeader="false"
        Width="100%" Height="100%" BackColor="White" BorderColor="#E7E7FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="3" EnableModelValidation="True" 
        GridLines="None" OnRowDataBound="gvUsers_RowDataBound" >

        <Columns>

            <asp:TemplateField ShowHeader="false">
                <ItemTemplate><asp:Image runat="server" ID="avatarURL" /></ItemTemplate>
                <ItemStyle HorizontalAlign="center" Width="70px" Height="66px"/>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Login Name" HeaderStyle-HorizontalAlign="Left">
                <ItemTemplate><asp:HyperLink ID="contentLink" runat="server" /></ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Full Name" HeaderStyle-HorizontalAlign="Left">
                <ItemTemplate><asp:Label ID="Name" runat="server" /></ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Email Address" HeaderStyle-HorizontalAlign="Left">
                <ItemTemplate><asp:Label ID="Email" runat="server" /></ItemTemplate>
                <ItemStyle HorizontalAlign="Left" />
            </asp:TemplateField>


            <asp:TemplateField Visible="false">
                <ItemTemplate><asp:Label ID="Login" runat="server" /></ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField Visible="false">
                <ItemTemplate><asp:Label ID="userURL" runat="server" /></ItemTemplate>
            </asp:TemplateField>

        </Columns> 

        <FooterStyle BackColor="#B5C7DE" ForeColor="#4A3C8C" BorderStyle="None" />
    </asp:GridView>
</asp:Content>

<asp:Content ID="cphFooter" ContentPlaceHolderID="cphFooter" runat="server"/>
