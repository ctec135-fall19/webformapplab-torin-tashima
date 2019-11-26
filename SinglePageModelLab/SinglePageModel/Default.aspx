<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SinglePageModel.Default" %>
<%@ Import Namespace="AutoLotDAL.Models" %>
<%@ Import Namespace="AutoLotDAL.DataOperations" %>

<!DOCTYPE html>

<script runat="server">
    public IEnumerable<AutoLotDAL.Models.Shipper> GetData()
    {
        return new InventoryDAL().GetAllInventory();
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
        </div>
        <asp:GridView ID="GridView1" runat="server" ItemType="AutoLotDAL.Models.Shipper" SelectMethod="GetData">
        </asp:GridView>
    </form>
</body>
</html>
