<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<MVC.Models.TaskInfoWithLink>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	First
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <!-- can create an automatic submitter by constantly querying and then if it gets an "ok" from the server it can submit automatically.-->
    <script type="text/javascript">
        function addEventHandler(elem, eventType, handler) {
            if (elem.addEventListener) {
                elem.addEventListener(eventType, handler, false);
            } else if (elem.attachEvent) {
                elem.attachEvent('on' + eventType, handler);
            }
        }

        function show(which) {
            if (!which) return false;
            setTimeout(function () { which.style.display = "block"; }, 60000);
        }

        function hide(which) {
            if (!which) return false;
            which.style.display = "none";            
        }

    </script>
    <script type="text/javascript">
        window.onload = function () {
            hide(document.getElementById('submitLink'));
            addEventHandler(document.getElementById('showSubmit'), 'click', function (e) {
                show(document.getElementById('submitLink'));
            });
        };
    </script>
    <h2>Do not close this window until you complete the HIT.</h2>
    <h2>Please click on the following link. It will open a new tab or window.</h2>
    <h2><a target="_blank" href="/home/<%= this.Model.Link %>?workerId=<%= this.Model.WorkerId %>&assignmentId=<%= this.Model.AssignmentId %>&culture=<%= this.Model.Culture %>" id="showSubmit">click me!</a></h2>
    <br />
    <!--div id="submitLink">
        <h3>Do not click on the following link until instructed to submit your HIT (or your work will be rejected) !!!!</h3>
        <h3><a href="https://www.mturk.com/mturk/externalSubmit?workerId=<%= this.Model.WorkerId %>&assignmentId=<%= this.Model.AssignmentId %>&culture=<%= this.Model.Culture %>" >submit HIT</a></h3>
    </div-->


</asp:Content>
