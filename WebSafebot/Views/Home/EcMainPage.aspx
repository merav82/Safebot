<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MVC.Models.TaskInfoModel>" %>

<asp:Content ID="refresh" ContentPlaceHolderID="scripting" runat="server">
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Main Interaction Page
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">

        setInterval(function () {
            $.ajax({
                type: 'POST',
                url: '/Home/EcGetUpdate/',
                datatype: 'json',
                data: {
                    workerId: '<%=Model.WorkerId %>',
                    assignmentId: '<%=Model.AssignmentId %>',
                    lastupdate: $("#lastUpdated").val()
                },
                success: function (res) {
                    treatResponse(res)
                },
                error: function (msg) {
                }
            });
        }, 500);

        function treatResponse(res) {
            //var response = jQuery.parseJSON(res);
            var response = eval('(' + res + ')');
            //alert(response.Value + response.Name);
            if (response.HasUpdate)
            {
             //   $('#currentTask').html("<h1>" + "<font color=\"red\">" + response.CurrentTask + "</font>" + "</h2>");
             //   $('#currentTask').append("<h2>" + "Tasks completed: " + "<font color=\"red\">" + response.TasksCompleted + "</font>" + " of " + response.NumOfTasksRequired + "<h2>");

            
                if (response.IsComplete)
                {
                    window.location.href = "EcGameComplete?assignmentId=<%= this.Model.AssignmentId %>&workerId=<%= this.Model.WorkerId %>>";
                    return true;
                }

                $("#PGChat").html(response.Text);
                $("#lastUpdated").val(response.UpdateTime);
                $('#PGChat').scrollTop($('#PGChat')[0].scrollHeight);
            }
            return false
        }


        function say(sentence) {
            $.ajax({
                type: 'POST',
                url: '/Home/EcSaySomething/',
                datatype: 'json',
                data: {
                    workerId: '<%=Model.WorkerId %>',
                    assignmentId: '<%=Model.AssignmentId %>',
                    sentence: sentence
                },
                success: function (res) {
                    treatResponse(res)
                },
                error: function (msg) {
                }
            });
            return false;
        }

        $(document).ready(function () {
            $('input.disablecopypaste').bind('copy paste', function (e) {
                e.preventDefault();
            });
        })

        $(function () {
            $(".sayingSentence").click(function () {
                userSaid = $("#sentence7").val();
                $("#sentence7").val('');
                return say(userSaid);
            });
        });

    </script>
    <script type="text/javascript">
        window.onload = function () {
                //$("#PGChat").height = "10px";
                //document.getElementById("PGChat").style.height = "150px";
                //say("");
        };
    </script>
    <div id='EcTop'>
        <div id="currentTask" />
    </div>
    <div id="texthp">
        <div id="PGleft">
                        <!--h3>
                Your notes:
            </!--h3>
            <img src="../../Content/notes.jpg" alt="Notes" style="width:190px;height:220px;"/-->
            <br />
            <br />
            <a href="EcGameComplete?assignmentId=<%= this.Model.AssignmentId %>&workerId=<%= this.Model.WorkerId %>">End conversation</a> <br />
            <br />
            <a target="_blank" href="EcInstructionsOnly?assignmentId=<%= this.Model.AssignmentId %>&workerId=<%= this.Model.WorkerId %>">Instructions (opens in new tab)</a>
            <div id='message'>
            </div>
        </div>
        <div id="PGright">
            <div id="PGChat">
            </div>
            <div id="PGbottom">
                <%= Html.Hidden("lastUpdated") %>
                <div id="messageDiv">
                    <form name="sayingSentence" action="" method="post">
                    <p>
                        <input name="sentence7" id="sentence7" size="98" autocomplete="off" class="disablecopypaste"/>
                        <input type="submit" class="sayingSentence" value="Say" />
                    </p>
                    </form>
                    <div style="text-align:right ">
                        <a href="EcGameComplete?assignmentId=<%= this.Model.AssignmentId %>&workerId=<%= this.Model.WorkerId %>">End conversation</a> <br />
                    </div>
                </div>
                 <div id='endMessage'>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
