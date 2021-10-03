<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MVC.Models.EcTaskInfoGameMode>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Instructions, Questionnaire and Test
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Instructions</h2>

    <% if (Model.IsLearningMode) { %>
        <% Response.WriteFile("/Views/home/EcInstructionsText.txt");%>
    <% } else { %>
        <% Response.WriteFile("/Views/home/EcInstructionsTextNoLearning.txt");%>
    <%} %>

</asp:Content>

