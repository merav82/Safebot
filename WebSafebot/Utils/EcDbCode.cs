using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;

namespace MVC.Utils
{
    public static class EcDbCode
    {

        internal static void EcNewWorker(
        string workerId,
        int age,
        string country,
        string education,
        string gender,
        int culture)
        {
            DbCode.GnNewWorker(workerId, age, country, education, gender, culture, "EcNewWorkerSP");
        }

        internal static bool EcIsWorkerAllowed(string workerId, string assignmentId)
        {
            return DbCode.GnIsWorkerAllowed(workerId, assignmentId, "EcIsWorkerAllowedSP");
        }


        internal static void EcNewGame(string workerId, string assignmentId, bool isLearningMode)
        {
            using (var db = new MTurkDBEntities())
            {
                db.Database.ExecuteSqlCommand("insert into EcGames (workerId,assignmentId,isLearningMode) values ({0},{1},{2})", workerId, assignmentId, isLearningMode);
            }
        }

        static public int EcGetGameId(string workerId, string assignmentId)
        {
            bool dummy;
            return EcGetGameId(workerId, assignmentId, out dummy);
        }

        static public int EcGetGameId(string workerId, string assignmentId, out bool isLearningMode)
        {
            int gameId = -1;
            isLearningMode = false;
            using (var db = new MTurkDBEntities())
            {
                var game = (from d in db.EcGames
                            where d.workerId == workerId && d.assignmentId == assignmentId && !d.finished
                            select new { d.gameId, d.isLearningMode }).FirstOrDefault();
                if (game != null)
                {
                    gameId = game.gameId;
                    isLearningMode = game.isLearningMode;
                }
            }
            return gameId;
        }


        internal static void EcGetTaskInfo(int gameId, out int tasksCompleted, out bool isComplete, out string currentTask)
        {
            using (var db = new MTurkDBEntities())
            {
                var gameInfo = (from g in db.EcGames
                                where g.gameId == gameId
                                select new { g.tasksCompleted, g.isComplete, g.currentTask }).FirstOrDefault();
                tasksCompleted = gameInfo.tasksCompleted;
                isComplete = gameInfo.isComplete;
                currentTask = gameInfo.currentTask;
            }
        }



        internal static void EcEndQuest(string workerId, string assignmentId, int program, int smart, int interest, int enjoy, int offensive, int meaningless, int real, int instructionsRead, int instructNotRead, string comments, string hitCode)
        {
            if (comments == null)
                comments = string.Empty;
            if (comments.Length >= 500)
                comments = comments.Substring(0, 499);

            using (var db = new MTurkDBEntities())
            {
                if (db.EcQuests.Any(e => e.workerId == workerId && e.assignmentId == assignmentId))
                {
                    foreach (EcQuest entity in db.EcQuests.Where(e => e.workerId == workerId && e.assignmentId == assignmentId).ToList())
                    {
                        db.EcQuests.Remove(entity);
                    }
                }
                db.EcQuests.Add(new EcQuest { workerId = workerId, assignmentId = assignmentId, program = program, smart = smart, interest = interest, instructionsRead = instructionsRead, instructNotRead = instructNotRead, enjoy = enjoy, offensive = offensive, meaningless = meaningless, real = real, comments = comments, hitCode = hitCode });
                db.SaveChanges();
                //db.Database.ExecuteSqlCommand("insert into EcQuest (workerId,assignmentId,program,smart,understood,mobile,comments) values ({0},{1},{2},{3},{4},{5},{6})", workerId, assignmentId, program, smart, understood, mobile, comments);

            }

        }


        internal static void EcAddSentence(int gameId, bool isAgentTalk, string sentence)
        {
            using (var db = new MTurkDBEntities())
            {
                //db.EcChats.Add(new EcChat { gameId = gameId, isAgentTalk = isAgentTalk, sentence = sentence, time = DateTime.Now });
                //db.SaveChanges();
                //making trouble: "an error occurred while updating the entries"
                db.Database.ExecuteSqlCommand("insert into EcChat (gameId,isAgentTalk,sentence) values ({0},{1},{2})", gameId, isAgentTalk, sentence);
            }
        }

        internal static System.Text.StringBuilder EcGetFullChat(int gameId)
        {
            System.Text.StringBuilder chatAsString = new System.Text.StringBuilder();
            using (var db = new MTurkDBEntities())
            {
                var chatList = (from d in db.EcChats
                                where d.gameId == gameId
                                orderby d.time
                                select new { isAgentTalk = d.isAgentTalk, d.sentence }).ToList();
                foreach (var s in chatList)
                {
                    string sentence = s.sentence;
                    chatAsString.Append(EcSettings.EcGetFixedSentence(sentence, s.isAgentTalk));
                }
                chatAsString.Append("<br/>");
            }
            return chatAsString;
        }

        //TODO: work on this!
        internal static void EcSetTasksCompleted(int gameId, int tasksCompleted, string currentTask, bool isCompleteAll, string lastTaskCompleted, out bool newTaskCompleted)
        {
            newTaskCompleted = false;
            using (var db = new MTurkDBEntities())
            {
                int oldTasksCompleted = (from g in db.EcGames
                                         where g.gameId == gameId
                                         select g.tasksCompleted).FirstOrDefault();
                if (oldTasksCompleted < tasksCompleted)
                    newTaskCompleted = true;

                db.Database.ExecuteSqlCommand("update EcGames set tasksCompleted = {0}, currentTask = {1}, isComplete = {2} where gameId={3}", tasksCompleted, currentTask, isCompleteAll, gameId);
                //the IGNORE_DUP_KEY is set to on, on the DB, so don't need to worry about multiple insertions
                //the EcTasksCompleted table is actually used only for convenience (so I won't need to look at the Java logs).
                db.Database.ExecuteSqlCommand("insert into EcTasksCompleted (gameId,taskName) values ({0},{1})", gameId, lastTaskCompleted);
                //db.EcTasksCompleteds.Add(new EcTasksCompleted { gameId = gameId, taskName = lastTaskCompleted, completedTime = DateTime.Now });
                //db.SaveChanges();
                if (newTaskCompleted && isCompleteAll)
                    db.Database.ExecuteSqlCommand("update EcGames set completeTime = {0} where gameId={1}", DateTime.Now, gameId);

            }
        }


        public static bool DidUserComplete(string workerId, string assignmentId)
        {
            bool isComplete = false;
            int gameId = EcDbCode.EcGetGameId(workerId, assignmentId);
            if (gameId == -1)
                throw new Exception("Error, no gameId.");
            /*int tasksCompleted;
            bool isComplete;
            string currentTask;
            EcDbCode.EcGetTaskInfo(gameId, out tasksCompleted, out isComplete, out currentTask);
            */
            using (var db = new MTurkDBEntities())
            {
                int sentenceCounter = (from g in db.EcChats
                                       where g.gameId == gameId && g.isAgentTalk == true
                                       select g).Count();

                if (sentenceCounter >= 10)
                    isComplete = true;
            }
            

            return isComplete;
        }






        internal static void UserFinished(string workerId, string assignmentId)
        {
            int gameId = EcDbCode.EcGetGameId(workerId, assignmentId);
            if (gameId != -1)
            {
                using (var db = new MTurkDBEntities())
                {
                    db.Database.ExecuteSqlCommand("update EcGames set finished = 1 where workerId={0} and assignmentId={1}", workerId, assignmentId);
                }
            }
        }
    }
}
