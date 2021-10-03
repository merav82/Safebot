using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC.Models;
using MVC.Utils;
using Amazon.WebServices.MechanicalTurk;
using Amazon.WebServices.MechanicalTurk.Domain;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;

namespace MVC.Controllers
{
    [HandleError]
    public partial class HomeController : Controller
    {



        private ActionResult GenericFirst(string workerId, string assignmentId, string hitId, string link, string culture, string checkLink=null)
        {
            try
            {
                if (!(DbCode.CheckValidString(workerId) && DbCode.CheckValidString(assignmentId) && DbCode.CheckValidString(hitId)))
                {
                    return View("Error");
                }

                if (assignmentId == "ASSIGNMENT_ID_NOT_AVAILABLE")
                {
                    ViewData["Message"] = "<h2>This HIT can only be taken once. Please accept HIT to proceed.</h2>";
                    if (checkLink != null)
                        ViewData["Message"] += "<p>If you aren't sure whether you have already taken this HIT or not, you may click <a target=\"_blank\" href=\"/home/GnTestIfAllowed?link=" + checkLink + "\" >here</a> to check. (You may also simply accept the HIT but if you have already taken the HIT you will be told to return it.)</p>";
                    return View("Empty");
                }
                if (assignmentId == null || hitId == null || workerId == null)
                    return View("Error");
                TaskInfoWithLink taskInfoModel = new TaskInfoWithLink()
                {
                    AssignmentId = assignmentId,
                    WorkerId = workerId,
                    Link = link,
                    Culture = culture
                };
                return View("GnFirst", taskInfoModel);
            }
            catch (Exception ex)
            {
                string message = "Function:GenericFirst workerId:" + ArgForMsg(workerId) + " assignmentId:" + ArgForMsg(assignmentId) + " hitId" + ArgForMsg(hitId) + " link: " + ArgForMsg(link) + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
                return View("Error");
            }
        }

        public ActionResult GnTestIfAllowed(string link)
        {
            try
            {
                if (!(DbCode.CheckValidString(link)))
                {
                    return View("Error");
                }
                TaskInfoWithLink taskInfoModel = new TaskInfoWithLink()
                {
                    AssignmentId = null,
                    WorkerId = null,
                    Link = link,
                    Culture = null
                };
                return View("GnTestIfAllowed", taskInfoModel);
            }
            catch (Exception ex)
            {
                string message = "Function:GenericFirst link:" + ArgForMsg(link) + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
                return View("Error");
            }
        }

        delegate bool checkAllowed(string workerId);
        private ActionResult GnCheckAllowed(string workerId, checkAllowed check)
        {
            try
            {
                bool isAllowed = check(workerId);
                if (!isAllowed)
                {
                    ViewData["Message"] = "<h2>Sorry, you have already taken this HIT and can't take it again.</h2>";
                    return View("Empty");
                }
                else
                {
                    ViewData["Message"] = "<h2>You have never taken this HIT and are welcome to take it!</h2>";
                    return View("Empty");
                }
            }
            catch (Exception ex)
            {
                string message = "Function:GnCheckAllowed workerId:" + ArgForMsg(workerId) + " " + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
                return View("Error");
            }
        }

 
        static private string ArgForMsg(string arg)
        {
            return arg == null ? "(null)" : arg;
        }

        const string closeAndReturn = "<h2>Please close this window and return the HIT.</h2><h3>Do <b>NOT</b> click on the \"Submit HIT\" link!</h3>";


        private bool IsNullEmptyOrError(string str)
        {
            return string.IsNullOrEmpty(str) || string.Compare(str, "error", true) == 0;
        }


        private static void ApproveAsThread(object assignemnetIdObj)
        {
            string assignemnetId = "Error in cast";
            try
            {
                assignemnetId = (string)assignemnetIdObj;
                Approve(assignemnetId);
            }
            catch (Exception ex)
            {
                string message = "Failed in ApproveAsThread " + ArgForMsg(assignemnetId) + ". Message:" + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
            }
        }

        private static bool Approve(string assignmentId)
        {
            const int sleepTime = 15 * 1000;
            Thread.Sleep(sleepTime);
            try
            {
                SimpleClient mTurkClient = new SimpleClient();
                int numTryToApprove = 30;
                for (int i = 1; i <= numTryToApprove; i++)
                {
                    try
                    {
                        mTurkClient.ApproveAssignment(assignmentId, "Thanks.");
                        return true;
                    }
                    catch (Exception)
                    {
                        if (i == numTryToApprove)
                            throw;
                        Thread.Sleep(2 * sleepTime);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = "Failed Approving for:" + ArgForMsg(assignmentId) + ". Message:" + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
            }
            return false;
        }










        public ActionResult Error(string error, string workerId, string assignmentId)
        {
            return View();
        }

        /// <summary>
        /// returns if got the lock or not
        /// </summary>
        internal static bool GetLock(string workerId, string assignmentId, int value=0)
        {
            return Lock(workerId, assignmentId, value, false);
        }

        internal static void ReleaseLock(string workerId, string assignmentId, int value=0)
        {
            Lock(workerId, assignmentId, value, true);
        }
        
        /// <summary>
        /// returns if got the lock or not (and if removed or not)
        /// </summary>
        private static bool Lock(string workerId, string assignmentId, int value, bool release)
        {
            string lockKey = LockKey(workerId, assignmentId, value);
            lock (locker)
            {
                if (locker.ContainsKey(lockKey))
                {
                    if (release)
                    {
                        locker.Remove(lockKey);
                        return true;
                    }
                    else
                        return false;
                }
                if (release)
                    return false;
                locker.Add(lockKey, -1);
                return true;
            }
        }

        private static string LockKey(string workerId, string assignmentId, int value)
        {
            string concStr = "|";
            return workerId + concStr + assignmentId + concStr+ value;
        }

        static Dictionary<string, int> locker = new Dictionary<string, int>();

















        struct ApproveAndBonusSt
        {
            public string WorkerId { get; set; }
            public string AssignmentId { get; set; }
            public float BonusAmount { get; set; }
            public string GameName { get; set; }
            public bool DontApprove { get; set; }
            public int UserBonusNumber { get; set; }
        }

        private void ApproveAndBonus(object gmApproveAndBonusObj)
        {
            try
            {
                ApproveAndBonusSt gmApproveAndBonusSt = (ApproveAndBonusSt)gmApproveAndBonusObj;
                bool approveSuccess = true;
                if (!gmApproveAndBonusSt.DontApprove)
                    approveSuccess = Approve(gmApproveAndBonusSt.AssignmentId);
                //send bonus
                const int sleepTime = 15 * 1000;
                Thread.Sleep(sleepTime);
                if (/*approveSuccess &&*/ gmApproveAndBonusSt.BonusAmount >= 0.01f) // since only one thread can approve, this solves double bonus grants in case this thread runs more than once.
                {
                    gmApproveAndBonusSt.BonusAmount = float.Parse(gmApproveAndBonusSt.BonusAmount.ToString("n02"));
                    bool okToGrant = DbCode.GnGrantingBonus(gmApproveAndBonusSt.WorkerId, gmApproveAndBonusSt.AssignmentId, gmApproveAndBonusSt.GameName, gmApproveAndBonusSt.BonusAmount, gmApproveAndBonusSt.UserBonusNumber);
                    if (okToGrant)
                    {
                        try
                        {
                            SimpleClient mTurkClient = new SimpleClient();
                            int numTryToApprove = 30;
                            for (int i = 1; i <= numTryToApprove; i++)
                            {
                                try
                                {
                                    mTurkClient.GrantBonus(gmApproveAndBonusSt.WorkerId, (decimal)gmApproveAndBonusSt.BonusAmount, gmApproveAndBonusSt.AssignmentId, "As promised...");
                                    try
                                    {
                                        DbCode.GnBonusSuccessOrDespair(gmApproveAndBonusSt.WorkerId, gmApproveAndBonusSt.AssignmentId, gmApproveAndBonusSt.UserBonusNumber, gmApproveAndBonusSt.GameName, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        string message = "GmApproveAndBonus writing to Db failed for workerId:" + ArgForMsg(gmApproveAndBonusSt.WorkerId) + " amount: " + ArgForMsg(gmApproveAndBonusSt.BonusAmount.ToString()) + " assignmentId" + ArgForMsg(gmApproveAndBonusSt.AssignmentId) + ". Message:" + ArgForMsg(ex.Message);
                                        DbCode.ExcepionMessage(message);
                                    }
                                    break;
                                }
                                catch (Exception)
                                {
                                    if (i == numTryToApprove)
                                    {
                                        DbCode.GnBonusSuccessOrDespair(gmApproveAndBonusSt.WorkerId, gmApproveAndBonusSt.AssignmentId, gmApproveAndBonusSt.UserBonusNumber, gmApproveAndBonusSt.GameName, false);
                                        throw;
                                    }
                                    Thread.Sleep(2 * sleepTime);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = "GmApproveAndBonus Bonus failed for workerId:" + ArgForMsg(gmApproveAndBonusSt.WorkerId) + " amount: " + ArgForMsg(gmApproveAndBonusSt.BonusAmount.ToString()) + " assignmentId" + ArgForMsg(gmApproveAndBonusSt.AssignmentId) + ". Message:" + ArgForMsg(ex.Message);
                            DbCode.ExcepionMessage(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = "GmApproveAndBonus(ext) Bonus" + ". Message:" + ArgForMsg(ex.Message);
                DbCode.ExcepionMessage(message);
            }
        }



    }
}
