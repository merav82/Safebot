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
    <!--
    <p>You will receive a bonus of $<%=Model.bonusAmount.ToString("n02") %> when you complete the main task (if you are unable to complete the task, you can click on the small "quit" link, your HIT will be approved, but you will not receive any bonus).</p>
    <p>(note that you can't copy-paste, since this system is intended to later be used for speech commands.)</p>
    <br />
    -->

    <h2>Questionnaire</h2>
    <form name="QuestionnaireAndTest" action="EcTestSubmit" method="get">
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
    <option value="rather not say">Rather not say</option>
	</select></p>
	<input type="hidden" name="assignmentId" value="<%= this.Model.AssignmentId %>"/>
	<input type="hidden" name="workerId" value="<%= this.Model.WorkerId %>"/>
    <input type="hidden" name="isLearningMode" value="<%= this.Model.IsLearningMode %>" />
	<br />

        <!--
    <h2>Test</h2>

	<p>Will the emails that you will be seeing during this experiment be real? Will you send out real emails?<br />
	<input type="radio" name="real" value="no" />
	No, everything will be done in a mock environment. All emails and names are fictional.<br />
	<input type="radio" name="real" value="yes1" />
	Yes, I will send real emails.<br />
	<input type="radio" name="real" value="yes2" />
	Yes, I will be seeing real emails from my real inbox.<br />
    <br />
    </p>

    <% if(this.Model.IsLearningMode) { %>
    <p>Can the computer agent learn new commands?<br />
    <input type="radio" name="teach" value="no" />
	No, that is impossible.<br />
    <input type="radio" name="teach" value="must" />
	Yes and I must teach it anything that it doesn't understand.<br />
	<input type="radio" name="teach" value="yes" />
	Yes. If the agent doesn't understand a command it will ask me if I want to teach it. Teaching it new commands may save me time (but I shouldn't attempt teaching it before the 13th training task).<br />
    <br />
    </p>
    <% } %>

    <p>How will you communicate with the computer agent?<br />
    <input type="radio" name="communicate" value="video" />
	Using both audio and video.<br />
    <input type="radio" name="communicate" value="audio" />
	Using audio only.<br />
	<input type="radio" name="communicate" value="text" />
	Using text on the bottom of the screen.<br />
    <br />
    </p>

    <p>How will you reply to an email?<br />
    <input type="radio" name="reply" value="reply" />
	I will use the same subject as the incoming email and use the sender as the recipient.<br />
    <input type="radio" name="reply" value="any" />
	I can use any subject.<br />
	<input type="radio" name="reply" value="body" />
	I must use the same body.<br />
    <br />
    </p>

    <p>How will you forward an email?<br />
    <input type="radio" name="forward" value="any" />
	I can use any subject and any body.<br />
    <input type="radio" name="forward" value="forward" />
	I will use the same subject <b>and</b> the same body as the incoming email.<br />
	<input type="radio" name="forward" value="body" />
	I must use the same body only, but am free to use a different subject.<br />
    <br />
    </p>

    <p> Which of the following sentences is correct:<br />
    <input type="radio" name="spelling" value="ok" />
	The agent never minds any spelling mistakes.<br />
    <input type="radio" name="spelling" value="correctly" />
	If I have spelling mistakes, the agent may not understand me. I must use apostrophes (') (e.g. current email's sender), but all other punctuation is optional.<br />
	<input type="radio" name="spelling" value="body" />
	All punctuation is optional including apostrophes (').<br />
    <br />
    </p>
    -->
    <h2>Consent Form</h2>
     <!--   Please click <a target="_blank" href="../../content/cmu-irb-consent-instuctable-agentMT.doc">here</a> to read the consent form (which will inform you of the goals of this research, data collected etc.).
    -->
        <p>
    <input type="checkbox" name="signed" />I agree to participate in this research study.</p>
    <p>Worker Id: <input type="text" name="txtWorkerId" /> </p>
    <p><input type="submit" value="Submit" /></p> 
    </form>
</asp:Content>

