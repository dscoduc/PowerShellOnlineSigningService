<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="PowerShellOnlineSigningService.Home" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PowerShell Online Signing Service</title>
    <link rel="stylesheet" href="style.css" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="header_wrapper">
            <div id="header">
                <h1>PowerShell Online Signing Service</h1>
            </div> <%--header--%>
        </div> <%--header_wrapper--%>

        <div id="content_wrapper">
            <div id="breadcrumb_wrapper" runat="server">
                <asp:TextBox ID="tbOwner" runat="server" />
                <span id="currentPath" runat="server"></span>
            </div>
            <asp:GridView ID="gvFiles" runat="server" AllowPaging="True" AutoGenerateColumns="False" CssClass="gvFiles" PageSize="20"
                Width="100%" Height="100%" BackColor="White" BorderColor="#E7E7FF" BorderStyle="Solid" BorderWidth="1px" CellPadding="3" EnableModelValidation="True" 
                GridLines="Horizontal" OnPageIndexChanging="gvFiles_PageIndexChanging" OnRowDataBound="gvFiles_RowDataBound" OnRowCommand="gvFiles_RowCommand" >
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

                <FooterStyle BackColor="#B5C7DE" ForeColor="#4A3C8C" />
                <PagerStyle BackColor="#E7E7FF" ForeColor="#4A3C8C" HorizontalAlign="Right" />
            </asp:GridView>
        </div> <%--content_wrapper--%> 

        <div id="footer_wrapper">
            <div id="serverInfo" runat="server" /> <%--serverinfo--%>
            <div id="userInfo" runat="server" /> <%--userinfo--%>
            <div id="middle_footer" runat="server" /> <%--middle_footer--%>
        </div> <%--footer_wrapper--%>
    </form>
</body>
</html>
