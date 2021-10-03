<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MVC.Models.TaskInfoWithLink>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Questionnaire
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Questionnaire</h2>
    <form name="QuestionnaireAndTest" action="<%=this.Model.Link %>" method="get">
    <p>Year of birth (four digits): <input type="text" name="age" /></p>
	<p>Country of Birth:<input type="text" name="country" /></p>
	<p>Education:<select name="education" id="educationDropDown" >
	<option value="error">(Please select your education)</option>
	<option value="none">None</option>
	<option value="elementary">Elementary School</option>
	<option value="jr">Jr. High School</option>
	<option value="high">High School</option>
	<option value="bachelor">Bachelor's Degree</option>
	<option value="master">Master's Degree</option>
	<option value="phd">PhD</option>
	</select></p>
	<p>Gender:<select name="gender" id="genderDropDown">
	<option value="error">(Please select your gender)</option>
	<option value="male">Male</option>
	<option value="female">Female</option>
    <option value="rather_not_say">Rather not say</option>
	</select></p>
	<input type="hidden" name="assignmentId" value="<%= this.Model.AssignmentId %>"/>
	<input type="hidden" name="workerId" value="<%= this.Model.WorkerId %>"/>
	<br />

    <p><input type="submit" value="Submit" /></p>
	</form>
</asp:Content>

