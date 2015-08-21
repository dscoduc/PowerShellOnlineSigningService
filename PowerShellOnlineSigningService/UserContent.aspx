<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="UserContent.aspx.cs" Inherits="PowerShellOnlineSigningService.UserContent" %>
<%@ MasterType VirtualPath="~/Main.master" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="cphHead" runat="server" />

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
    <div id="breadcrumb_wrapper" runat="server">

        <span id="currentPath" runat="server"></span>
    </div>
    <asp:GridView ID="gvFiles" runat="server" AllowPaging="True" AutoGenerateColumns="False" CssClass="gvFiles" PageSize="20" ShowHeader="false"
        Width="100%" Height="100%" BackColor="White" BorderColor="#E7E7FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="3" EnableModelValidation="True" 
        GridLines="Horizontal" OnPageIndexChanging="gvFiles_PageIndexChanging" OnRowDataBound="gvFiles_RowDataBound" >
        <Columns>

            <asp:TemplateField>
                <ItemTemplate><asp:Image runat="server" ID="typeImage" /></ItemTemplate>
                <ItemStyle HorizontalAlign="Center" Width="17px" />
            </asp:TemplateField>

            <asp:TemplateField>
                <ItemTemplate><asp:HyperLink ID="contentLink" runat="server" /></ItemTemplate>
                <ItemStyle HorizontalAlign="Left" Width="450px" />
            </asp:TemplateField>

            <asp:TemplateField>
                <ItemTemplate><asp:Label ID="Size" runat="server" CssClass="size" /></ItemTemplate>
                <ItemStyle HorizontalAlign="Right" Width="100px" />
            </asp:TemplateField>

            <asp:TemplateField Visible="false">
                <ItemTemplate><asp:Label ID="Path" runat="server" /></ItemTemplate>
            </asp:TemplateField>
                      
            <asp:TemplateField Visible="false">
                <ItemTemplate><asp:Label ID="Type" runat="server" /></ItemTemplate>
            </asp:TemplateField>

        </Columns> 

        <FooterStyle BackColor="#B5C7DE" ForeColor="#4A3C8C" BorderStyle="None" />
        <PagerStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" HorizontalAlign="Center" />
    </asp:GridView>
</asp:Content>

<asp:Content ID="cphFooter" ContentPlaceHolderID="cphFooter" runat="server"/>
