<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PowerShellOnlineSigningService.Default" %>

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
            <table id="settings_table">
                <tr>
                    <td class="tLabel">Repository Owner:</td>
                    <td><asp:TextBox runat="server" ID="tbRepoOwner" /></td>
                </tr>
                <tr>
                    <td class="tLabel">Repository:</td>
                    <td>
                        <asp:DropDownList ID="ddlRepositories" runat="server" AutoPostBack="True" OnSelectedIndexChanged="btnRefreshList_Click" />
                    </td>
                </tr>
                <tr>
                    <td class="tLabel"></td>
                    <td style="text-align:right;">
                        <asp:Button ID="btnSignFile" runat="server" Text="Refresh" ToolTip="Click to refresh file list" OnClick="btnRefreshList_Click" />
                    </td>
                </tr>
            </table>
           <div id="results">
                <span id="resultsInfo" runat="server" />
            </div> <%--results--%>
            <div id="downloads">
                <asp:Table runat="server" ID="tblFileList" />
            </div> <%--downloads--%>
        </div> <%--content_wrapper--%>
        <div id="footer_wrapper">
            <div id="serverInfo" runat="server" /> <%--serverinfo--%>
            <div id="userInfo" runat="server" /> <%--userinfo--%>
            <div id="middle_footer" runat="server" /> <%--middle_footer--%>
        </div> <%--footer_wrapper--%>
    </form>
</body>
</html>
