<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MVC.Models.TaskInfoModel>" %>
<asp:Content ID="refresh" ContentPlaceHolderID="scripting" runat="server">

</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Game
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">
    setInterval(function () {
        $.ajax({
            type: 'POST',
            url: '/Home/PgAJAXRquestToPlay',
            datatype: 'json',
            data: {
                workerId: '<%=Model.WorkerId %>',
                assignmentId: '<%=Model.AssignmentId %>'
            },
            success: function (res) {
                treatResponse(res)
            },
            error: function (msg) {
            }
        });
    }, 1000);

    function treatResponse(res) {
        //var response = jQuery.parseJSON(res);
        var response = eval('(' + res + ')');
        if (response.isIn == 1)
        {
            window.location.href = "PgPlayGame?assignmentId=<%= this.Model.AssignmentId %>&workerId=<%= this.Model.WorkerId %>";
            return true;
        }
        if (response.maxSecondsLeft <= 0)//voting
        {
            //window.location.href = "PgWaitedTooLong?assignmentId=<%= this.Model.AssignmentId %>&workerId=<%= this.Model.WorkerId %>";
            $("#endTime").show();
            $('#message').hide();
            return false;
        }
        else {
            var timeLeftSt = Math.floor((response.maxSecondsLeft / 60)) + ":" + (response.maxSecondsLeft % 60 < 10 ? "0" : "") + response.maxSecondsLeft % 60;
            $('#message').html("<h2>Maximum waiting time (usually MUCH less): " + "<font color=green>" + timeLeftSt + "</font></h2>");
            return false;
        }
    }
</script>
<script type="text/javascript">
    window.onload = function () {
        $("#endTime").hide();
    };
</script>
<h2>Please wait for other players to join...</h2>
<div id='message'></div>
<div id='endTime'>
    <form name="PgRequestToPlay" action="PgRequestToPlay" method="get">
    <input type="hidden" name="assignmentId" value="<%= this.Model.AssignmentId %>"/>
	<input type="hidden" name="workerId" value="<%= this.Model.WorkerId %>"/>
    <input type="submit" value="Wait Again" />
	</form>
    <br />
    <form name="PgWaitedTooLong" action="PgWaitedTooLong" method="get">
    <input type="hidden" name="assignmentId" value="<%= this.Model.AssignmentId %>"/>
	<input type="hidden" name="workerId" value="<%= this.Model.WorkerId %>"/>
    <input type="submit" value="End!" />
	</form>
</div>
</asp:Content>
