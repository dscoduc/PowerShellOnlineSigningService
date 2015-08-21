<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OwnerList.aspx.cs" Inherits="PowerShellOnlineSigningService.OwnerList" %>

<!DOCTYPE html>

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
        </div> <%--content_wrapper--%> 

        <div id="footer_wrapper">
            <div id="serverInfo" runat="server" /> <%--serverinfo--%>
            <div id="userInfo" runat="server" /> <%--userinfo--%>
            <div id="middle_footer" runat="server" /> <%--middle_footer--%>
        </div> <%--footer_wrapper--%>
    </form>
</body>
</html>
