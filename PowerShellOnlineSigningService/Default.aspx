<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PowerShell Online Signing Service</title>
    <link rel="stylesheet" href="css/style.css" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="header_wrapper">
            <div id="header">
                <h3>PowerShell Online Signing Service</h3>
            </div> <!-- header -->
        </div> <!-- header_wrapper -->
        <!-- <div id="content_description">
    
        </div> content_description -->
        <div id="content_body">
            <div id="upload">
                <asp:FileUpload ID="uploadFile" runat="server" Multiple="Multiple" ToolTip="Upload script to be signed." Width="349px" />
                <br /><br />
                <asp:Button ID="btnSignFile" runat="server" Text="Sign Script" OnClick="btnSignFile_Click"/>
            </div>
        </div> <!-- content_body -->
        <div id="results">
            <span id="resultsInfo" runat="server" />
            <asp:Table runat="server" ID="tblResults" />
        </div>
        <div id="downloads">
            <asp:Table runat="server" ID="tblFileList" />
        </div>
        <div id="footer_wrapper">
            <div id="footer">
                <div id="serverInfo" runat="server"></div> <!-- serverinfo -->
                <div id="userInfo" runat="server" ><!-- Placeholder for after user authenticates --></div><!-- userinfo -->
                <div id="middle_footer" runat="server"></div>
            </div> <!-- footer -->
        </div> <!-- footer_wrapper -->
    </form>
</body>
</html>
