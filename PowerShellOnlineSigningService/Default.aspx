<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PowerShellOnlineSigningService.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PowerShell Online Signing Service</title>
    <%--<link rel="stylesheet" href="style.css" type="text/css" />--%>
    <style type="text/css">
        body{
            width: 800px;
            margin:0 auto;
            padding: 0;
        }
        #header_wrapper {
            margin:0 auto;
            padding:0;
            background: url('images/banner.jpg') no-repeat right;
            width: 100%;
            height: 226px;
        }
        #header {
            width: 500px;
            margin: 0px 30px;
            padding: 30px 15px;
            border-bottom: 0px solid #ccc;
            text-align: left;
            color: white;
            padding-top: 150px;
            padding-bottom: 0px;
        }
        #header h1 {
            margin-bottom: 0;
            padding-bottom: 0;
        }
        #content_wrapper {
            margin: 10px auto 0px; 
            width: 100%;
            min-height: 500px;
        }
        #settings_table {
            width: 400px;
            margin: 20px auto;
            padding-bottom: 15px;
        }
        #settings_table .tLabel {
            width: 100px;
            color: #666;
            text-align: right;
        }
        #settings_table input[type='text'] {
            width: 99%;
            font-size: 1.1em;
        }

        #results {
            width: 600px; 
            margin: 10px auto;        
        }

        #downloads { 
            padding: 15px; 
            color: #666; 
            width: 600px; 
            margin: 10px auto;
            min-height: 200px;
            border: 0px #ccc solid;
        }
        #downloads table { 
            width: 100%;
        }
        #downloads table th {
            border-bottom:1px solid #ccc;
            font-weight: bold;
        }
        #downloads table td {
            border-bottom:1px dotted #ccc;
        }
        #downloads .size { 
            color: #666;  
            float: right; 
            padding-right: 10px;
        }
        #footer_wrapper {
            border-top: 1px solid #ccc;
            width: 100%;
            height: 40px;
            margin: 0 auto;
            padding: 10px 0;
        }
        #serverInfo {
            float: right;
            font-size: x-small;
            color: ghostwhite;
        }
        #userInfo {
            float: left;
            font-size: x-small;
            color: #666;
        }
        #middle_footer {
            text-align: center;
        }

    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div id="header_wrapper">
            <div id="header">
                <h1>PowerShell Online Signing Service</h1>
            </div> <!-- header -->
        </div> <!-- header_wrapper -->
        <div id="content_wrapper">
            <table id="settings_table">
                <tr>
                    <td class="tLabel">Repo Owner:</td>
                    <td><asp:TextBox runat="server" ID="tbRepoOwner" /></td>
                </tr>
                <tr>
                    <td class="tLabel">Repository:</td>
                    <td><asp:TextBox runat="server" ID="tbRepository" /></td>
                </tr>
                <tr>
                    <td class="tLabel"></td>
                    <td style="text-align:right;"><asp:Button ID="btnSignFile" runat="server" Text="Refresh" ToolTip="Click to refresh file list" OnClick="btnRefreshList_Click" /></td>
                </tr>
            </table>
           <div id="results">
                <span id="resultsInfo" runat="server" />
            </div>
            <div id="downloads">
                <asp:Table runat="server" ID="tblFileList" />
            </div>
        </div> <!-- Content -->
        <div id="footer_wrapper">
            <div id="serverInfo" runat="server" /> <!-- serverinfo -->
            <div id="userInfo" runat="server" /><!-- userinfo -->
            <div id="middle_footer" runat="server" />
        </div> <!-- footer_wrapper -->
    </form>
</body>
</html>
