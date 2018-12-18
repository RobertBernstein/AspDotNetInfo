<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="aspnetinfo.aspx.cs" Inherits="AspDotNetInfo.AspDotNetInfo" %>
<%@ Import Namespace="System.IO" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ASP.NET Information v1.0</title>
    <link rel="stylesheet" type="text/css" href="AspDotNetInfo.css" /> 
</head>
<body>
    <form id="form1" runat="server">
    <div class="center">
        <h1>
            ASP.NET Information v1.0</h1>
        <h2>
            Server Information</h2>
        <asp:DataGrid runat="server" ID="ServerInformation" AutoGenerateColumns="false" Width="600px"
            ShowHeader="False">
            <Columns>
                <asp:BoundColumn DataField="key" ItemStyle-CssClass="e" />
                <asp:BoundColumn DataField="value" ItemStyle-CssClass="v" />
            </Columns>
        </asp:DataGrid>
        <h2>
            Request Headers</h2>
        <asp:Repeater ID="RequestHeaders" runat="server">
            <HeaderTemplate>
                <table style="border: 0; padding: 3em" width="600">
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="e">
                        <%# Container.DataItem %>
                    </td>
                    <td class="v">
                        <%# Request.Headers[(String)Container.DataItem] %>&nbsp;
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
        <h2>
            Environment Variables</h2>
        <asp:DataGrid runat="server" ID="EnvironmentVars" AutoGenerateColumns="False" Width="600px"
            ShowHeader="False">
            <Columns>
                <asp:BoundColumn DataField="key" ItemStyle-CssClass="e" />
                <asp:BoundColumn DataField="value" ItemStyle-CssClass="v" />
            </Columns>
        </asp:DataGrid>
        <h2>
            Server Variables</h2>
        <asp:Repeater ID="ServerVariables" runat="server">
            <HeaderTemplate>
                <table style="border: 0; padding: 3em;" width="600">
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="e">
                        <%# Container.DataItem %>
                    </td>
                    <td class="v">
                        <%# Request.ServerVariables[(String)Container.DataItem] %>&nbsp;
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
        <h2>
            Logical Drives</h2>
        <asp:Repeater ID="LogicalDrives" runat="server">
            <HeaderTemplate>
                <table style="border: 0; padding: 3em" width="600">
                    <tr>
                        <td class="h">
                            Drive Letter
                        </td>
                        <td class="h">
                            Drive Type
                        </td>
                        <td class="h">
                            Volume Label
                        </td>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="e">
                        <%# ((KeyValuePair<String,DriveInfo>)Container.DataItem).Key %>
                    </td>
                    <td class="v">
                        <%# ((KeyValuePair<String, DriveInfo>)Container.DataItem).Value.DriveType.ToString() %>&nbsp;
                    </td>
                    <td class="v">
                        <%# ((KeyValuePair<String, DriveInfo>)Container.DataItem).Value.VolumeLabel %>&nbsp;
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
        <h2>
            Installed Programs</h2>
        <asp:DataGrid runat="server" ID="InstalledPrograms" Width="600px" ShowHeader="False"
            ItemStyle-CssClass="v" />
        <br />
        <br />
        <table style="border: 0; padding: 3em" width="600">
            <tr>
                <td class="e" style="text-align: center; font-size: large">
                    (C) 2009-2013 Tardis Technologies
                </td>
            </tr>
            <tr>
                <td class="v" style="text-align: center; font-size: medium">
                    Portions of code and formatting were borrowed from <a href="http://web.archive.org/web/20080309155944/http://semichaos.com/articles/aspnet1/aspnetinfo.aspx"
                        target="_blank">http://semichaos.com/articles/aspnet1/aspnetinfo.aspx</a> and
                    phpinfo(). &nbsp; .NET Runtime version information was collected from <a href="http://msdn.microsoft.com/en-us/kb/kbarticle.aspx?id=318785"
                        target="_blank">http://msdn.microsoft.com/en-us/kb/kbarticle.aspx?id=318785</a>, <a href="http://dzaebel.net/NetVersions.htm"
                            target="_blank">http://dzaebel.net/NetVersions.htm</a>, and <a href="http://en.wikipedia.org/wiki/List_of_.NET_Framework_versions"
                                target="_blank">http://en.wikipedia.org/wiki/List_of_.NET_Framework_versions</a>.
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
