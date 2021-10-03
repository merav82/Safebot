<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MVC.Models.TaskAndCultureModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	First
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Please click on the following link. It will open a new tab or window.</h2>
    <h2><a target="_blank" href="/home/requ?workerId=<%= this.Model.WorkerId %>&assignmentId=<%= this.Model.AssignmentId %>&culture=<%= this.Model.Culture %>" >click me!</a></h2>

</asp:Content>
