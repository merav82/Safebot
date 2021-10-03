using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC.Utils
{
    static public class EcSettings
    {
        public const int agentServerPort = 5000;
        public const string serverName = "localhost";
       // public const int agentServerPort = 18892;


        //public const string contextSayToAgent = "say";
        public const string userSaysParam = "userSays";
        public const string contextEmailAndExperiment = "emailAndExperiment";
        public const string gameIdParam = "gameId";
        public const string actionParam = "action";
        public const string sayToAgentStr = "sayToAgent";
        public const string learningParam = "learningParam";
        public const string whenContainedSendAnotherRequest = "this takes several seconds"; //IAllUserActions.resendNewRequest; //TODO: not elegant that this is hard coded
        public const string resendRequested = "resendRequested";


        public const string getTasksInfoStr = "getTasksInfo";
        public const string newGameJoinedStr = "newGameJoined";
        public const string gameScoreStr = "gameScore";
        public const string gameTaskStr = "gameTask";
        public const string recentTaskCompletedStr = "recentTaskCompleted";

        public const int numOfPlayers = 2;
        public const int numOfTasksRequired = 25;//24;//25; //for non-teachers require only 24
        public const float bonusAmount = 2.5f;//1.7f;
        //public static bool isLearningMode = true;//true;

        public static string EcGetFixedSentence(string sentence, bool isAgentTalk)
        {
            return "<font color=\"" + playerColors[isAgentTalk ? 1 : 0] + "\">" + "<b>" + playerNames[isAgentTalk ? 1 : 0] + ":" + "</b> " + sentence.Trim().Replace("\n", "<br/>") + "</font>" + "<br/>" + (isAgentTalk ? "<br/>" : "");
        }

        public static string[] playerNames = { "User", "Chatbot" };
        public static string[] playerColors = { "Purple", "OliveDrab" };//, "Brown", "Darkorange" };
    }
}