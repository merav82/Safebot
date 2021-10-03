<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MVC.Models.RequestModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	HitOk
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Thank you for accepting and submitting the HIT, it should be approved in less than one minute.</h2>
        <%= this.Model.MessageInfo %>
        <h3><a href="Bonus?workerId=<%= this.Model.WorkerId %>&assignmentId=<%= this.Model.AssignmentId %>&go=true" >Go Bonus!</a></h3>
        <h3><a name="" href="Bonus?workerId=<%= this.Model.WorkerId %>&assignmentId=<%= this.Model.AssignmentId %>&go=false" >No Thanks</a></h3>

</asp:Content>
