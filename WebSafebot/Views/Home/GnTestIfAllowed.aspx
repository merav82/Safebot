<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MVC.Models.TaskInfoWithLink>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Check
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Please enter your worker id and click on submit</h2>
    <p>(This information is required, since Mechanical Turk does not provide your worker id until you accept the HIT.)</p>
    <form name="GnTestIfAllowed" action="/home/<%=this.Model.Link %>" method="get">
	<p>WorkerId:<input type="text" name="workerId" size="20" style="width:150px;"/></p>
	<br />
    <p><input type="submit" value="Submit" /></p>
	</form>

</asp:Content>
