using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC.Utils;
using MVC.Models;
using System.Threading;
using Amazon.WebServices.MechanicalTurk;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web.Configuration;

namespace MVC.Controllers
{
    //TR
    [HandleError]
    public partial class HomeController : Controller
    {

        [HttpPost]
        public JsonResult EcGetUpdate(string workerId, string assignmentId, string lastUpdate /*null to force update*/)
        {
            try
            {
                DateTime dtlastUpdate = DateTime.MinValue;
                bool dtSuccess = false;
                if (lastUpdate != null)
                    dtSuccess = DateTime.TryParse(lastUpdate, out dtlastUpdate);
                if (!dtSuccess)
                    dtlastUpdate = DateTime.MinValue;

                int gameId = EcDbCode.EcGetGameId(workerId, assignmentId);
                if (gameId == -1)
                    throw new Exception("Error, no gameId.");
                bool hasUpdate;
                string text;
                DateTime updateTime;
                bool isComplete;
                int tasksCompleted;
                string currentTask;
                EcUpdateInter(gameId, dtlastUpdate, false, out tasksCompleted, out isComplete, out hasUpdate, out text, out currentTask, out updateTime);
                return Json(new
                {
                    Text = text,
                    HasUpdate = hasUpdate,
                    UpdateTime = updateTime.ToString("o"),
                    TasksCompleted = tasksCompleted,
                    IsComplete = isComplete,
                    NumOfTasksRequired = EcSettings.numOfTasksRequired,
                    CurrentTask = currentTask
                });
            }
            catch (Exception ex)
            {
                string message = "Function:PgUpdate workerId:" + ArgForMsg(workerId) + " assignmentId:" + ArgForMsg(assignmentId) + " lastUpdate:" + lastUpdate + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
                return Json(new object()); //I don't really know what I should return.
            }
        }

        private void EcUpdateInter(int gameId, DateTime lastUpdate, bool isAgent, out int tasksCompleted,
            out bool isComplete, out bool hasUpdate, out string text, out string currentTask, out DateTime updateTime)
        {
            isComplete = false;
            hasUpdate = false;
            text = string.Empty;
            updateTime = DateTime.MinValue;
            tasksCompleted = 0;
            currentTask = "";
            try
            {

                DateTime lastUpdateInGameList = DateTime.Now;//if read failed - update.
                bool congratulate = false;
                lock (gamesList)
                {
                    if (gamesList.ContainsKey(gameId))
                    {
                        lastUpdateInGameList = gamesList[gameId].lastUpdateGame;
                        congratulate = gamesList[gameId].congratulate;
                        if (congratulate)
                        {
                            gamesList[gameId].congratulate = false;
                            gamesList[gameId].lastUpdateGame = lastUpdateInGameList.AddSeconds(5);
                        }
                    }
                    else
                        lastUpdateInGameList = DateTime.MinValue;
                }
                bool hastoUpdate = lastUpdate < lastUpdateInGameList;
                if (hastoUpdate)
                {
                    hasUpdate = true;
                    updateTime = lastUpdateInGameList;
                    text = EcDbCode.EcGetFullChat(gameId).ToString();
                    EcDbCode.EcGetTaskInfo(gameId, out tasksCompleted, out isComplete, out currentTask);
                    if (congratulate)
                        currentTask = "Well Done!!!";
                }
            }
            catch (Exception ex)
            {
                string message = "Function:PgUpdateInter workerId:" + " gameId" + gameId + " lastUpdate:" + lastUpdate + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
            }
        }

        class perGame
        {
            public DateTime lastUpdateGame = DateTime.MinValue;
            public DateTime gameStart = DateTime.Now;
            public bool congratulate;

            private int gameId;

            public perGame(int gameId)
            {
                this.gameId = gameId;
            }
        }

        static private Dictionary<int, perGame> gamesList = new Dictionary<int, perGame>();


        [HttpPost]
        public JsonResult EcSaySomething(string workerId, string assignmentId, string sentence)
        {
            try
            {

                if (!String.IsNullOrEmpty(sentence))
                {
                    bool learningAgent;
                    int gameId = EcDbCode.EcGetGameId(workerId, assignmentId, out learningAgent);
                    if (gameId == -1)
                        throw new Exception("Error, no gameId.");
                    EcDbCode.EcAddSentence(gameId, false, sentence);
                    lock (gamesList)
                    {
                        if (!gamesList.ContainsKey(gameId)) //shouldn't really happen because already added already
                            gamesList.Add(gameId, new perGame(gameId));
                        gamesList[gameId].lastUpdateGame = DateTime.Now;
                    }
                    //call Java Agent in background and get response
                    Task.Run(() => SayToJavaAgentAndCheckUpdate(gameId, sentence, learningAgent));
                }
                return EcGetUpdate(workerId, assignmentId, null);
            }
            catch (Exception ex)
            {
                string message = "Function:PgSaySomething workerId:" + ArgForMsg(workerId) + " assignmentId:" + ArgForMsg(assignmentId) + "sentence" + sentence + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
                //return View("Error");
                return Json(new object()); //I don't really know what I should return.
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="sentence">set to null for resend</param>
        private void SayToJavaAgentAndCheckUpdate(int gameId, string sentence, bool learningAgent)
        {
            try
            {
                SayToJavaAgent(gameId, sentence, learningAgent);
                //need to give a moment for the Java agent and DB to finish updating gameId status
                //sleeping for a millisecond will also make sure that the update time here is newer than the update time above (when adding user's sentence)
                //this anyway runs as a separate thread.
                System.Threading.Thread.Sleep(2);
                CheckUpdateFromJava(gameId);
                lock (gamesList)
                {
                    gamesList[gameId].lastUpdateGame = DateTime.Now;//.AddMilliseconds(2);
                }
            }
            catch (Exception ex)
            {
                string message = "Function:SayToAgentAndCheckUpdate gameId:" + gameId + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="sentence">set to null for resend</param>
        private void SayToJavaAgent(int gameId, string sentence, bool learningAgent)
        {
            try
            {
                string encodedSentence = HttpUtility.UrlEncode(sentence);
                bool investBot = learningAgent;
                Response r = new Response();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Response));

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url;
                string serverName = WebConfigurationManager.AppSettings["serverName"];
                string portNumber = WebConfigurationManager.AppSettings["serverPort"];
                url = "http://" + serverName + ":" + portNumber + "/api/getResponse?id=" + gameId + "&sentence=" + encodedSentence + "&investBot=" + investBot;

                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Timeout = 30000000;
                objRequest.Method = WebRequestMethods.Http.Get;


                //Retrieve the Response returned from the NVP API call to PayPal. 
                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                string result;
                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }

                // convert string to stream
                byte[] byteArray = Encoding.UTF8.GetBytes(result);
                MemoryStream stream1 = new MemoryStream(byteArray);


                r = (Response)ser.ReadObject(stream1);
                if (investBot)
                    r.chat_type = "safebot";
                else
                    r.chat_type = "simpleBot";
                string responseString = r.response;

                EcDbCode.EcAddSentence(gameId, true, responseString);
                
            }
            catch (Exception ex)
            {
                string message = "Function:SayToJavaAgen gameId:" + gameId + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
            }
        }

        private static void CheckUpdateFromJava(int gameId)
        {
            /*
            string url = "http://localhost:" + EcSettings.agentServerPort + "/" + EcSettings.contextEmailAndExperiment;

            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values[EcSettings.gameIdParam] = gameId.ToString();
                values[EcSettings.actionParam] = EcSettings.getTasksInfoStr;

                var response = client.UploadValues(url, values);

                string jsonString = Encoding.Default.GetString(response);
                JObject json = JObject.Parse(jsonString);
                int tasksCompleted = int.Parse(json.GetValue(EcSettings.gameScoreStr).ToString());
                string currentTask = json.GetValue(EcSettings.gameTaskStr).ToString();
                string lastTaskCompleted = json.GetValue(EcSettings.recentTaskCompletedStr).ToString();
                bool congratulate;
                EcDbCode.EcSetTasksCompleted(gameId, tasksCompleted, currentTask, tasksCompleted >= EcSettings.numOfTasksRequired, lastTaskCompleted, out congratulate);
                if (congratulate)
                {
                    lock (gamesList)
                    {
                        if (gamesList.ContainsKey(gameId))
                        {
                            gamesList[gameId].congratulate = congratulate;
                        }
                    }

                }
            }*/
        }

        private static void AddNewGameToJavaAndArr(int gameId, bool learningAgent)
        {
            /*
            string url = "http://localhost:" + EcSettings.agentServerPort + "/" + EcSettings.contextEmailAndExperiment;

            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values[EcSettings.gameIdParam] = gameId.ToString();
                values[EcSettings.actionParam] = EcSettings.newGameJoinedStr;
                values[EcSettings.learningParam] = learningAgent.ToString();

                client.UploadValues(url, values);
            }
            */

            CheckUpdateFromJava(gameId);
            lock (gamesList)
            {
                if (!gamesList.ContainsKey(gameId))
                {
                    gamesList.Add(gameId, new perGame(gameId));
                }
                gamesList[gameId].lastUpdateGame = DateTime.Now;
            }
        }

        public ActionResult EcFirst(string workerId, string assignmentId, string hitId)
        {
            return GenericFirst(workerId, assignmentId, hitId, "EcInstructions", 1.ToString());
        }

        public ActionResult EcInstructions(string workerId, string assignmentId)
        {
            try
            {
                if (!(DbCode.CheckValidString(workerId) &&
                    DbCode.CheckValidString(assignmentId)
                    ))
                {
                    return View("Error");
                }
                //bool isAllowed = EcDbCode.EcIsWorkerAllowed(workerId, assignmentId);
                bool isAllowed = true;

                if (!isAllowed)
                {
                    ViewData["Message"] = "<h2>Sorry, you can't take this HIT again</h2>" + closeAndReturn;//"<h2>Sorry, you have joined us in the past.</h2><h2>This HIT is currently opened for new participants only.</h2>" + closeAndReturn;
                    return View("Empty");
                }

                Random rand = new Random();
                double trueProbability = 0.5;
                bool isLearningMode = rand.NextDouble() < trueProbability;
                
                // Random random = new Random(workerId.GetHashCode());
                //Random random = new Random();
                //bool isLearningMode = random.NextBool();
               // bool isLearningMode = true;
            //    if (random.NextDouble() > 0.5)
            //        isLearningMode = true;
                return View("EcInstructions", new EcTaskInfoGameMode { WorkerId = workerId, AssignmentId = assignmentId, IsLearningMode = isLearningMode, bonusAmount = EcSettings.bonusAmount });
            }
            catch (Exception ex)
            {
                string message = "Function:PgInstructions workerId:" + ArgForMsg(workerId) + " assignmentId:" + ArgForMsg(assignmentId) + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
                return View("Error");
            }
        }

        public ActionResult EcInstructionsOnly(string workerId, string assignmentId)
        {
            bool isLearningMode;
            int gameId = EcDbCode.EcGetGameId(workerId, assignmentId, out isLearningMode);
            return View("EcInstructionsOnly", new EcTaskInfoGameMode { WorkerId = workerId, AssignmentId = assignmentId, IsLearningMode = isLearningMode, bonusAmount = EcSettings.bonusAmount });
        }

        public ActionResult EcTestSubmit(
             string workerId,
             string assignmentId,
             string isLearningMode,
             string age,
             string country,
             string education,
             string gender,
             string real,
             string teach,
             string communicate,
             string reply,
             string forward,
             string spelling,
             string signed,
             string txtWorkerId)
        {
            try
            {
                workerId = txtWorkerId;
                assignmentId = workerId;
                bool isAllowed = EcDbCode.EcIsWorkerAllowed(workerId, assignmentId);
                if (!isAllowed)
                {
                    ViewData["Message"] = "<h2>Sorry, you can't take this HIT again</h2>" + closeAndReturn;//"<h2>Sorry, you have joined us in the past.</h2><h2>This HIT is currently opened for new participants only.</h2>" + closeAndReturn;
                    return View("Empty");
                }

                bool bisLearningMode = true;
                bool success = Boolean.TryParse(isLearningMode, out bisLearningMode);

             //   Random random = new Random(workerId.GetHashCode());
             //   bool bisLearningMode = false;
             //   if (random.NextDouble() > 0.5)
             //       bisLearningMode = true;

    //            
                if (!(DbCode.CheckValidString(workerId) &&
                    DbCode.CheckValidString(assignmentId) &&
                    DbCode.CheckValidString(country) &&
                    DbCode.CheckValidString(education) &&
                    DbCode.CheckValidString(gender) && success
                    ))
                {
                    return View("Error");
                }

                if (signed != "on")
                {
                    ViewData["Message"] = "<h2>You did not sign the consent form, please go back and sign it by clicking on the check-box.</h2>";
                    DbCode.ExcepionMessage("Consent not signed. workerId:" + ArgForMsg(workerId) + ".", "info");
                    return View("Empty");
                }

                if (IsNullEmptyOrError(workerId))
                {
                    ViewData["Message"] = "<h2>You did not fill your Worker Id, please go back and correct it.</h2>";
                    return View("Empty");
                }

                /*
                if (real != "no" ||
                    (teach != "yes" && bisLearningMode) ||
                    communicate != "text" ||
                    reply != "reply" ||
                    forward != "forward" ||
                    spelling != "correctly")
                {
                    ViewData["Message"] = "<h2>You made at least one mistake in the test, please go back and correct it.</h2>";
                    DbCode.ExcepionMessage("Mistakes in test. workerId:" + ArgForMsg(workerId) + " real:" + ArgForMsg(real) + " teach:" + ArgForMsg(teach) + " communicate:" + ArgForMsg(communicate) + " reply:" + ArgForMsg(reply) + " forward:" + ArgForMsg(forward) + " spelling:" + ArgForMsg(spelling), "info");
                    return View("Empty");
                }
                */

                int nAge = 0;
                if (
                    IsNullEmptyOrError(workerId) ||
                    IsNullEmptyOrError(assignmentId) ||
                    IsNullEmptyOrError(age) ||
                    IsNullEmptyOrError(country) ||
                    IsNullEmptyOrError(education) ||
                    IsNullEmptyOrError(gender) ||
                    !int.TryParse(age, out nAge)
                    )
                {
                    ViewData["Message"] = "<h2>There was an error reading some of your data (personal information), please go back and correct it.</h2>";
                    return View("Empty");
                }
                NewWorkerJoined(workerId, assignmentId, country, education, gender, nAge, bisLearningMode);
                return View("EcMainPage", new TaskInfoModel { WorkerId = workerId, AssignmentId = assignmentId });
                //return PgRequestToPlay(workerId, assignmentId);
            }
            catch (Exception ex)
            {
                string message = "Function:RrTestSubmit workerId:" + ArgForMsg(workerId) + " assignmentId:" + ArgForMsg(assignmentId) + " " + ArgForMsg(ex.Message) +
                                " age:" + ArgForMsg(age) + " country:" + ArgForMsg(country) + " education:" + ArgForMsg(education) + " gender:" + ArgForMsg(gender);
                DbCode.ExcepionMessage(message);
                return View("Error");
            }
        }

        private static void NewWorkerJoined(string workerId, string assignmentId, string country, string education, string gender, int nAge, bool learningAgent)
        {
            EcDbCode.EcNewWorker(workerId, nAge, country, education, gender, 1);
            bool isLearningAgent = false;
            int gameId = EcDbCode.EcGetGameId(workerId, assignmentId, out isLearningAgent);
            if (gameId == -1)
            {
                EcDbCode.EcNewGame(workerId, assignmentId, learningAgent);
                gameId = EcDbCode.EcGetGameId(workerId, assignmentId, out isLearningAgent);
                EcDbCode.EcAddSentence(gameId, true, "let's chat, you start");
                if (gameId == -1)
                    throw new Exception("error, no gameId");
            }
            Task.Run(() => AddNewGameToJavaAndArr(gameId, isLearningAgent));
        }

        private static string GenerateHitNum ()
        {
            string[] lines = System.IO.File.ReadAllLines(WebConfigurationManager.AppSettings["hitCodeLoc"]);

            var random = new Random();
            string hitCode = string.Empty;
            for (int i = 0; i < 15; i++)
            {
                if (i == 2)
                    hitCode = String.Concat(hitCode, lines[0]);
                else
                    if (i == 4)
                        hitCode = String.Concat(hitCode, lines[1]);
                    else
                        if (i == 9)
                            hitCode = String.Concat(hitCode, lines[2]);
                        else
                            if (i == 11)
                                hitCode = String.Concat(hitCode, lines[3]);
                            else
                                if (i == 13)
                                    hitCode = String.Concat(hitCode, lines[4]);
                                else
                                    hitCode = String.Concat(hitCode, random.Next(10).ToString());
            }
            return hitCode;
        }

        public ActionResult EcGameComplete(string workerId, string assignmentId)
        {
            try
            {
                bool isComplete = EcDbCode.DidUserComplete(workerId, assignmentId);
                String message;
                if (isComplete)
                {
                    message = "Well Done!!!";
                }
                else
                {
                    message = "Sorry to see you leaving.<br/>" +
                        "You can still click back and continue chatting with the chatbot";
                    DbCode.ExcepionMessage("Player quit. workerId:" + workerId + " assignmentId:" + assignmentId, "info");
                }
                ViewData["Message"] = message;
                return View("EcEndQuestionnaire", new TaskInfoModel { WorkerId = workerId, AssignmentId = assignmentId});
            }
            catch (Exception ex)
            {
                string message = "Function:PgGetEndQuest workerId:" + ArgForMsg(workerId) + " assignmentId:" + ArgForMsg(assignmentId) + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
                return View("Error");
            }
        }

        public ActionResult EcEndQuestSubmit(
        string workerId,
        string assignmentId,
        string program,
        string smart,
        string offensive,
        string meaningless,
        string real,
        string interest,
        string enjoy,
        string instructionsRead,
        string instructNotRead,
        string comments)
        {
            try
            {
                if (!(DbCode.CheckValidString(workerId) && DbCode.CheckValidString(assignmentId)))
                {
                    return View("Error");
                }
                if (string.IsNullOrEmpty(assignmentId) || string.IsNullOrEmpty(workerId))
                {
                    return View("Error");
                }

                int nprogram = 0;
                int nsmart = 0;
                int noffensive = 0;
                int nmeaningless = 0;
                int nreal = 0;
                int ninterest = 0;
                int nenjoy = 0;
                int ninstructionsRead = 0;
                int ninstructNotRead = 0;
                //returns 0 if program is null or other
                int.TryParse(program, out nprogram);
                int.TryParse(smart, out nsmart);
                int.TryParse(offensive, out noffensive);
                int.TryParse(meaningless, out nmeaningless);
                int.TryParse(real, out nreal);
                int.TryParse(interest, out ninterest);
                int.TryParse(enjoy, out nenjoy);
                int.TryParse(instructionsRead, out ninstructionsRead);
                int.TryParse(instructNotRead, out ninstructNotRead);


                string hitCode = GenerateHitNum();

                EcDbCode.EcEndQuest(workerId, assignmentId, nprogram, nsmart, ninterest, nenjoy ,noffensive, nmeaningless, nreal, ninstructionsRead, ninstructNotRead, comments, hitCode);
                return View("GnLastSubmitHit", new TaskInfoWithLink { AssignmentId = assignmentId, WorkerId = workerId, HitCode = hitCode, Link = "EcFinish" });
            }
            catch (Exception ex)
            {
                string message = "Function:PgEndQuestSubmit workerId:" + ArgForMsg(workerId) + " assignmentId:" + ArgForMsg(assignmentId) + " program:" + ArgForMsg(program) + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
                return View("Error");
            }
        }

        public ActionResult EcFinish(string workerId, string assignmentId)
        {
            try
            {
                if (!(DbCode.CheckValidString(workerId) && DbCode.CheckValidString(assignmentId)))
                {
                    return View("Error");
                }

                if (assignmentId == null || workerId == null)
                    return View("Error");
                bool isComplete = EcDbCode.DidUserComplete(workerId, assignmentId);
                EcDbCode.UserFinished(workerId, assignmentId);
                float bonusAmount = isComplete ? EcSettings.bonusAmount : 0;
                Thread th = new Thread(new ParameterizedThreadStart(ApproveAndBonus));
                th.Start(new ApproveAndBonusSt
                {
                    WorkerId = workerId,
                    AssignmentId = assignmentId,
                    BonusAmount = bonusAmount,
                    GameName = "EmailC",
                    UserBonusNumber = 1
                }
                    );

                return View("GnFinish");
            }
            catch (Exception ex)
            {
                string message = "Function:EcFinish workerId:" + ArgForMsg(workerId) + " assignmentId:" + ArgForMsg(assignmentId) + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
                return View("Error");
            }
        }

    }
}
