using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace MVC
{
    namespace Utils
    {
        static public class DbCode
        {
            

            public static string getConString()
            {
                string conString = ConfigurationManager.ConnectionStrings["MyCon"].ConnectionString;
                return conString;
            }


            //public static bool serverDst = false;
            // make sure no one is trying to attack.
            public static bool CheckValidString(string s)
            {
                if (s == null)
                    return true;
                if (s.Length >= 50)
                    return false; //string too long

                // if the string contains a ' it might contain an attack on the DB.
                //      all strings are injected to a command inside ' '.
                int Idx = s.IndexOf("'");
                return (Idx == -1);
            }


            public static void GnNewWorker(string workerId, int age, string country, string education, string gender, int culture, string spName)
            {
                if (!(DbCode.CheckValidString(workerId) &&
                    DbCode.CheckValidString(country) &&
                    DbCode.CheckValidString(education) &&
                    DbCode.CheckValidString(gender))
                    )
                {
                    return;
                }
                using (SqlConnection conn = new SqlConnection(getConString()))
                {
                    conn.Open();
                    // command
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = spName;
                    DbCode.AddParmToCmd(cmd, "workerId", workerId);
                    DbCode.AddParmToCmd(cmd, "age", age);
                    DbCode.AddParmToCmd(cmd, "country", country);
                    DbCode.AddParmToCmd(cmd, "education", education);
                    DbCode.AddParmToCmd(cmd, "gender", gender);
                    DbCode.AddParmToCmd(cmd, "culture", culture);
                    cmd.ExecuteNonQuery();
                }
            }

            internal static bool GnIsWorkerAllowed(string workerId, string assignmentId, string spName)
            {
                if (!(DbCode.CheckValidString(workerId) && DbCode.CheckValidString(assignmentId)))
                {
                    return false;
                }
                using (SqlConnection conn = new SqlConnection(getConString()))
                {
                    conn.Open();
                    // command
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = spName;
                    DbCode.AddParmToCmd(cmd, "workerId", workerId);
                    DbCode.AddParmToCmd(cmd, "assignmentId", assignmentId);
                    //output parameter:
                    SqlParameter sqlparmOpenedForNew = new SqlParameter();
                    sqlparmOpenedForNew.Direction = ParameterDirection.Output;
                    sqlparmOpenedForNew.ParameterName = "isAllowed";
                    sqlparmOpenedForNew.SqlDbType = SqlDbType.Int;
                    cmd.Parameters.Add(sqlparmOpenedForNew);
                    cmd.ExecuteNonQuery();
                    int isAllowed = 0;
                    try
                    {
                        if (sqlparmOpenedForNew.Value != System.DBNull.Value)
                        {
                            isAllowed = (int)sqlparmOpenedForNew.Value;
                        }
                    }
                    catch (Exception)
                    {
                    }

                    return isAllowed != 0;
                }
            }

            internal static bool GmIsNewAssignmentId(string assignmentId)
            {
                if (!DbCode.CheckValidString(assignmentId))
                {
                    return false;
                }
                using (SqlConnection conn = new SqlConnection(getConString()))
                {
                    conn.Open();
                    // command
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GmIsNewAssignmentIdSP";
                    DbCode.AddParmToCmd(cmd, "assignmentId", assignmentId);
                    //output parameter:
                    SqlParameter sqlparmOpenedForNew = new SqlParameter();
                    sqlparmOpenedForNew.Direction = ParameterDirection.Output;
                    sqlparmOpenedForNew.ParameterName = "isNew";
                    sqlparmOpenedForNew.SqlDbType = SqlDbType.Int;
                    cmd.Parameters.Add(sqlparmOpenedForNew);
                    cmd.ExecuteNonQuery();
                    int isNew = 0;
                    try
                    {
                        if (sqlparmOpenedForNew.Value != System.DBNull.Value)
                        {
                            isNew = (int)sqlparmOpenedForNew.Value;
                        }
                    }
                    catch (Exception)
                    {
                    }

                    return isNew != 0;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="workerId"></param>
            /// <param name="assignmentId"></param>
            /// <param name="gameName"></param>
            /// <param name="bonusAmount"></param>
            /// <returns>whether it is OK to grand this bonus (or that it was already granted)</returns>
            internal static bool GnGrantingBonus(string workerId, string assignmentId, string gameName, float bonusAmount, int userBonusNumber)
            {
                using (var db = new MTurkDBEntities())
                {
                    if (db.GnBonus.Any(e => e.workerId == workerId && e.assignmentId == assignmentId && e.gameName == gameName)) //&& e.grantedSuccefully
                    {
                        return false;
                    }
                    db.GnBonus.Add(new GnBonu() { assignmentId = assignmentId, workerId = workerId, bonusAmount = bonusAmount, tryingToGrant = true, grantedSuccefully = false, gameName = gameName, createdTime = DateTime.Now, userBonusNumber = userBonusNumber });
                    db.SaveChanges();
                }
                return true;
            }

            internal static void GnBonusSuccessOrDespair(string workerId, string assignmentId, int userBonusNumber, string gameName, bool bonusGrantedSuccessfully)
            {
                if (!DbCode.CheckValidString(workerId) || !DbCode.CheckValidString(assignmentId))
                {
                    return;
                }

                using (var db = new MTurkDBEntities())
                {
                    GnBonu gnBonu = (from GnBonu in db.GnBonus
                                     where GnBonu.assignmentId == assignmentId && GnBonu.workerId == workerId && GnBonu.gameName == gameName && GnBonu.userBonusNumber == userBonusNumber
                                     select GnBonu).Single();
                    if (bonusGrantedSuccessfully)
                        gnBonu.grantedSuccefully = true;
                    else
                        gnBonu.tryingToGrant = false; //won't try granting anymore if failed too many times.

                    db.SaveChanges();
                }
            }



            public static void AssignmentApproved(string workerId, string assignmentId)
            {
                if (!DbCode.CheckValidString(workerId) || !DbCode.CheckValidString(assignmentId))
                { 
                    return;
                }

                using (SqlConnection conn = new SqlConnection(getConString()))
                {
                    conn.Open();
                    // command
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "AssignmentApprovedSP";
                    DbCode.AddParmToCmd(cmd, "workerId", workerId);
                    DbCode.AddParmToCmd(cmd, "assignmentId", assignmentId);
                    cmd.ExecuteNonQuery();
                }
            }

            public static void AddNewWorker(
                string workerId,
                TimeSpan timeZoneSpan,
                bool dst,
                int age,
                string country,
                string education,
                string gender,
                string religious,
                int culture,
                int scheduleType,
                string marital,
                int children
                )
            {
                if (!(DbCode.CheckValidString(workerId) &&
                    DbCode.CheckValidString(country) &&
                    DbCode.CheckValidString(education) &&
                    DbCode.CheckValidString(gender) &&
                    DbCode.CheckValidString(religious) &&
                    DbCode.CheckValidString(marital))
                    )
                { 
                    return;
                }
                int dstInt = dst ? 1 : 0;
                using (SqlConnection conn = new SqlConnection(getConString()))
                {
                    conn.Open();
                    // command
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "NewWorkerSP";
                    DbCode.AddParmToCmd(cmd, "workerId", workerId);
                    DbCode.AddParmToCmd(cmd, "timeZone", timeZoneSpan.TotalHours);
                    DbCode.AddParmToCmd(cmd, "dst", dstInt);
                    DbCode.AddParmToCmd(cmd, "age", age);
                    DbCode.AddParmToCmd(cmd, "country", country);
                    DbCode.AddParmToCmd(cmd, "education", education);
                    DbCode.AddParmToCmd(cmd, "gender", gender);
                    DbCode.AddParmToCmd(cmd, "religious", religious);
                    DbCode.AddParmToCmd(cmd, "culture", culture);
                    DbCode.AddParmToCmd(cmd, "scheduleType", scheduleType);
                    DbCode.AddParmToCmd(cmd, "marital", marital);
                    DbCode.AddParmToCmd(cmd, "children", children);
                    cmd.ExecuteNonQuery();
                }
            }


            public static float GetTotalPrize()
            {
                float totalBonusPrize = 0;
                using (SqlConnection conn = new SqlConnection(getConString()))
                {
                    conn.Open();
                    // command
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GetTotalPrizeSP";
                    SqlParameter sqlparmTotalPrize = new SqlParameter();
                    sqlparmTotalPrize.Direction = ParameterDirection.Output;
                    sqlparmTotalPrize.ParameterName = "totalBonusPrize";
                    sqlparmTotalPrize.SqlDbType = SqlDbType.Real;
                    cmd.Parameters.Add(sqlparmTotalPrize);
                    cmd.ExecuteNonQuery();
                    if (sqlparmTotalPrize.Value != null && sqlparmTotalPrize.Value != System.DBNull.Value)
                    {
                        totalBonusPrize = (float)sqlparmTotalPrize.Value;
                    }
                }
                return totalBonusPrize;
            }

  
            public enum NewParticipantsEnum { Closed = 0, Opened = 1, WorkerNotAllowed = -1 }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="culture">0 for any culture</param>
            /// <param name="workerId">workerId that wants to join, null or empty if not available.</param>
            /// <returns>1 opened, 0 closed, -1 worker not allowed</returns>
            internal static NewParticipantsEnum IsOpenedForNewParticipants(int culture, string workerId)
            {
                if (workerId == null)
                    workerId = string.Empty;
                using (SqlConnection conn = new SqlConnection(getConString()))
                {
                    conn.Open();
                    // command
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "IsOpenedForNewSP";
                    DbCode.AddParmToCmd(cmd, "culture", culture);
                    DbCode.AddParmToCmd(cmd, "workerId", workerId);
                    //output parameter:
                    SqlParameter sqlparmOpenedForNew = new SqlParameter();
                    sqlparmOpenedForNew.Direction = ParameterDirection.Output;
                    sqlparmOpenedForNew.ParameterName = "openedForNew";
                    sqlparmOpenedForNew.SqlDbType = SqlDbType.Int;
                    cmd.Parameters.Add(sqlparmOpenedForNew);
                    cmd.ExecuteNonQuery();
                    int openedForNew = 0;
                    try
                    {
                        if (sqlparmOpenedForNew.Value != System.DBNull.Value)
                        {
                            openedForNew = (int)sqlparmOpenedForNew.Value;
                        }
                    }
                    catch (Exception)
                    {
                    }

                    return (NewParticipantsEnum)openedForNew;
                }

            }

            /// <summary>
            /// exception safe
            /// </summary>
            /// <param name="message"></param>
            public static void ExcepionMessage(string message, string level = "severe")
            {
                try
                {
                    using (var db = new MTurkDBEntities())
                    {
                        db.Database.ExecuteSqlCommand("insert into GnExceptions (message,logLevel) values ({0},{1})", message,level);
                    }
                }
                catch (Exception)
                {
                }
            }

            internal static void AddParmToCmd(SqlCommand cmd, string parmName, object value)
            {
                SqlParameter parm = new SqlParameter();
                parm.ParameterName = parmName;
                parm.Value = value;
                cmd.Parameters.Add(parm);
            }

            internal static SqlParameter AddOutParm(SqlCommand cmd, string parmName, SqlDbType type)
            {
                SqlParameter sqlparm = new SqlParameter();
                sqlparm.Direction = ParameterDirection.Output;
                sqlparm.ParameterName = parmName;
                sqlparm.SqlDbType = type;
                if (type == SqlDbType.NChar)
                    sqlparm.Size = 10;
                cmd.Parameters.Add(sqlparm);
                return sqlparm;
            }


            internal static bool IsNewAssignmentId(string assignmentId)
            {
                if (assignmentId == null)
                    return false;
                using (SqlConnection conn = new SqlConnection(getConString()))
                {
                    conn.Open();
                    // command
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "NumOfAssignmentIdSP";
                    DbCode.AddParmToCmd(cmd, "assignmentId", assignmentId);
                    //output parameter:
                    SqlParameter sqlparmOpenedForNew = new SqlParameter();
                    sqlparmOpenedForNew.Direction = ParameterDirection.Output;
                    sqlparmOpenedForNew.ParameterName = "numAssginmentId";
                    sqlparmOpenedForNew.SqlDbType = SqlDbType.Int;
                    cmd.Parameters.Add(sqlparmOpenedForNew);
                    cmd.ExecuteNonQuery();
                    int numAssginmentId = 1; //number of times the assignmentId appears in the DB
                    try
                    {
                        if (sqlparmOpenedForNew.Value != System.DBNull.Value)
                        {
                            numAssginmentId = (int)sqlparmOpenedForNew.Value;
                        }
                    }
                    catch (Exception)
                    {
                    }

                    return numAssginmentId == 0;
                }

            }
        }
    }
}
