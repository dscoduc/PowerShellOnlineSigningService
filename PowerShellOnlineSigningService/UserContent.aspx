<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="UserContent.aspx.cs" Inherits="PowerShellOnlineSigningService.UserContent" %>
<%@ MasterType VirtualPath="~/Main.master" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="cphHead" runat="server" />

<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
    <asp:GridView ID="gvFiles" runat="server" AutoGenerateColumns="False" CssClass="gvFiles" GridLines="None"
        CellPadding="3" EnableModelValidation="True" ShowHeader="false" OnRowDataBound="gvFiles_RowDataBound" >
        <Columns>

            <asp:TemplateField ItemStyle-CssClass="master_image">
                <ItemTemplate><asp:Image runat="server" ID="typeImage" /></ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField ItemStyle-CssClass="gvFiles_link master_link">
                <ItemTemplate><asp:HyperLink ID="contentLink" runat="server" /></ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField ItemStyle-CssClass="gvFiles_size">
                <ItemTemplate><asp:Label ID="Size" runat="server" /></ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField Visible="false">
                <ItemTemplate><asp:Label ID="Path" runat="server" /></ItemTemplate>
            </asp:TemplateField>
                      
            <asp:TemplateField Visible="false">
                <ItemTemplate><asp:Label ID="Type" runat="server" /></ItemTemplate>
            </asp:TemplateField>

        </Columns> 
    </asp:GridView>
</asp:Content>

<asp:Content ID="cphFooter" ContentPlaceHolderID="cphFooter" runat="server"/>
