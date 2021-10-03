<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MVC.Models.TaskInfoModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	End
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
 
    <h2><%= ViewData["Message"] %></h2>
    <br />
    <h4>Just a few more questions...</h4>
    <form name="EndQuestionnaire" action="EcEndQuestSubmit" method="get">
    
<p>
    What is your level of familiarity with software/computer programming (please choose one)?<br />
    <input type="radio" name="program" value="1" />
    None.<br />
    <input type="radio" name="program" value="2" />
    Very little.<br />
    <input type="radio" name="program" value="3" />
    Some background from high-school.<br />
    <input type="radio" name="program" value="4" />
    Some background from college/university.<br />
    <input type="radio" name="program" value="6" />
    Bachelor (or other degree) with a major or minor in software, electrical or computer engineering or similar.<br />
    <input type="radio" name="program" value="5" />
    Significant knowledge, but mostly from other sources.<br />
</p>

    <h3>To what extent do you agree with the following statements:</h3>

    <p>1) The interaction with the chatbot was interesting. <br />
        <br />
	<input type="radio" name="interest" value="7" />
	Strongly agree.<br />
    <input type="radio" name="interest" value="6" />
	Agree.<br />
    <input type="radio" name="interest" value="5" />
	Slightly agree.<br />
	<input type="radio" name="interest" value="4" />
	Neither agree nor disagree.<br />
    <input type="radio" name="interest" value="3" />
	Slightly disagree.<br />
	<input type="radio" name="interest" value="2" />
	Disagree.<br />
    <input type="radio" name="interest" value="1" />
	Strongly disagree.
	<br/>
    <br />
	</p>	

    <p>2) The interaction with the chatbot was enjoyable. <br />
        <br />
	<input type="radio" name="enjoy" value="7" />
	Strongly agree.<br />
    <input type="radio" name="enjoy" value="6" />
	Agree.<br />
    <input type="radio" name="enjoy" value="5" />
	Slightly agree.<br />
	<input type="radio" name="enjoy" value="4" />
	Neither agree nor disagree.<br />
    <input type="radio" name="enjoy" value="3" />
	Slightly disagree.<br />
	<input type="radio" name="enjoy" value="2" />
	Disagree.<br />
    <input type="radio" name="enjoy" value="1" />
	Strongly disagree.
	<br/>
    <br />
	</p>	


	<p>3) The chatbot is smart.<br />
        <br />
	<input type="radio" name="smart" value="7" />
	Strongly agree.<br />
    <input type="radio" name="smart" value="6" />
	Agree.<br />
    <input type="radio" name="smart" value="5" />
	Slightly agree.<br />
	<input type="radio" name="smart" value="4" />
	Neither agree nor disagree.<br />
    <input type="radio" name="smart" value="3" />
	Slightly disagree.<br />
	<input type="radio" name="smart" value="2" />
	Disagree.<br />
    <input type="radio" name="smart" value="1" />
	Strongly disagree.
	<br/>
    <br />
	</p>

    <p>4) The chatbot used offending, spiteful language or hate speech.<br />
    <br />
	<input type="radio" name="offensive" value="7" />
	Strongly agree.<br />
    <input type="radio" name="offensive" value="6" />
	Agree.<br />
    <input type="radio" name="offensive" value="5" />
	Slightly agree.<br />
	<input type="radio" name="offensive" value="4" />
	Neither agree nor disagree.<br />
    <input type="radio" name="offensive" value="3" />
	Slightly disagree.<br />
	<input type="radio" name="offensive" value="2" />
	Disagree.<br />
    <input type="radio" name="offensive" value="1" />
	Strongly disagree.
	<br/>
    <br />
	</p>

     <p>5) The chatbot used meaningless responses.<br />
    <br />
	<input type="radio" name="meaningless" value="7" />
	Strongly agree.<br />
    <input type="radio" name="meaningless" value="6" />
	Agree.<br />
    <input type="radio" name="meaningless" value="5" />
	Slightly agree.<br />
	<input type="radio" name="meaningless" value="4" />
	Neither agree nor disagree.<br />
    <input type="radio" name="meaningless" value="3" />
	Slightly disagree.<br />
	<input type="radio" name="meaningless" value="2" />
	Disagree.<br />
    <input type="radio" name="meaningless" value="1" />
	Strongly disagree.
	<br/>
    <br />
	</p>


        <p>6) I felt like I was chatting with a real human.<br />
    <br />
	<input type="radio" name="real" value="7" />
	Strongly agree.<br />
    <input type="radio" name="real" value="6" />
	Agree.<br />
    <input type="radio" name="real" value="5" />
	Slightly agree.<br />
	<input type="radio" name="real" value="4" />
	Neither agree nor disagree.<br />
    <input type="radio" name="real" value="3" />
	Slightly disagree.<br />
	<input type="radio" name="real" value="2" />
	Disagree.<br />
    <input type="radio" name="real" value="1" />
	Strongly disagree.
	<br/>
    <br />
	</p>

     <p>7) I read the instructions before interacting with the chatbot.<br />
    <br />
	<input type="radio" name="instructionsRead" value="7" />
	Strongly agree.<br />
    <input type="radio" name="instructionsRead" value="6" />
	Agree.<br />
    <input type="radio" name="instructionsRead" value="5" />
	Slightly agree.<br />
	<input type="radio" name="instructionsRead" value="4" />
	Neither agree nor disagree.<br />
    <input type="radio" name="instructionsRead" value="3" />
	Slightly disagree.<br />
	<input type="radio" name="instructionsRead" value="2" />
	Disagree.<br />
    <input type="radio" name="instructionsRead" value="1" />
	Strongly disagree.
	<br/>
    <br />
	</p>


    <p>8) I totally ignored the instructions.<br />
    <br />
	<input type="radio" name="instructNotRead" value="7" />
	Strongly agree.<br />
    <input type="radio" name="instructNotRead" value="6" />
	Agree.<br />
    <input type="radio" name="instructNotRead" value="5" />
	Slightly agree.<br />
	<input type="radio" name="instructNotRead" value="4" />
	Neither agree nor disagree.<br />
    <input type="radio" name="instructNotRead" value="3" />
	Slightly disagree.<br />
	<input type="radio" name="instructNotRead" value="2" />
	Disagree.<br />
    <input type="radio" name="instructNotRead" value="1" />
	Strongly disagree.
	<br/>
    <br />
	</p>
    
    <p>Please let us know if you have any comments:<br />
    <textarea name="comments" rows="5" cols="100" ></textarea>
    </p>
    <input type="hidden" name="assignmentId" value="<%= this.Model.AssignmentId %>"/>
	<input type="hidden" name="workerId" value="<%= this.Model.WorkerId %>"/>
    <p><input type="submit" value="Submit" /></p>
	</form>
</asp:Content>