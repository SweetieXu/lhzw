﻿/*
 模块：报表管理
 编写：蒋正波
 时间：2016-10-20
 功能：查询数据
 */
using Asiatek.AjaxPager;
using Asiatek.Common;
using Asiatek.DBUtility;
using Asiatek.Model;
using Asiatek.Resource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Asiatek.BLL.MSSQL
{
    public class ReportBLL
    {
        #region  查询超速
        public static AsiatekPagedList<ReportListModel> GetPagedSpeed(ReportSearchModels model, int searchPage, int pageSize, int userID)
        {
            List<SqlParameter> paras = new List<SqlParameter>()
            {
                new SqlParameter("@tableName","Vehicles v"),
                new SqlParameter("@joinStr",@"INNER JOIN VehiclesTerminals vt on v.ID=vt.VehicleID 
INNER JOIN Terminals t on vt.TerminalID=t.ID 
INNER JOIN Exception_2 E ON E.TerminalCode=T.TerminalCode 
INNER JOIN Structures S ON S.ID=v.StrucID"),
                new SqlParameter("@pageSize",pageSize),
                new SqlParameter("@currentPage",searchPage),
                new SqlParameter("@orderBy","E.ID"),
                new SqlParameter("@showColumns",@"s.StrucName,v.VehicleName,e.GPSStartTime,e.GPSEndTime,
CAST(DATEDIFF(SS,GPSStartTime,GPSEndTime) AS VARCHAR) AS TIME"),
            };

            string conditionStr = " vt.IsPrimary=1 AND v.Status=0  ";//主终端

            #region   查询参数
            if (string.IsNullOrWhiteSpace(model.VehicleName))
            {
                conditionStr += @" AND v.VehicleName IN (
SELECT vt.VehicleName FROM dbo.Structures s INNER JOIN
(
SELECT v.VehicleName,v.StrucID,V.ID AS VID FROM dbo.Vehicles v 
INNER JOIN
(SELECT StrucID FROM dbo.StructureDistributionInfo
WHERE UserID=" + userID + @") AS temp1 ON v.StrucID=temp1.StrucID
WHERE v.Status=0 AND v.IsReceived=1  
UNION 
SELECT v.VehicleName,v.StrucID,v.ID AS VID FROM dbo.Vehicles v 
INNER JOIN
(SELECT VehicleID FROM dbo.VehicleDistributionInfo 
WHERE UserID=" + userID + @") AS temp1 ON v.ID=temp1.VehicleID
WHERE v.Status=0 AND v.IsReceived=1 
) AS vt ON s.ID=vt.StrucID)";
            }
            if (!string.IsNullOrWhiteSpace(model.VehicleName))
            {
                conditionStr += " AND v.VehicleName LIKE '%" + model.VehicleName + "%'";
            }
            if (!string.IsNullOrWhiteSpace(model.GPSStartTime))
            {
                conditionStr += " AND GPSStartTime>='" + model.GPSStartTime + "'";
                if (!string.IsNullOrWhiteSpace(model.GPSEndTime))
                {
                    conditionStr += " AND GPSEndTime<='" + model.GPSEndTime + "'";
                }
            }
            #endregion

            if (!string.IsNullOrWhiteSpace(conditionStr))
            {
                paras.Add(new SqlParameter("@conditionStr", conditionStr));
            }

            paras.Add(new SqlParameter()
            {
                ParameterName = "@totalItemCount",
                Direction = ParameterDirection.Output,
                SqlDbType = SqlDbType.Int
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@newCurrentPage",
                Direction = ParameterDirection.Output,
                SqlDbType = SqlDbType.Int
            });
            List<ReportListModel> list = ConvertToList<ReportListModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.StoredProcedure, "Proc_GetPagedDatas", paras.ToArray()));
            int totalItemCount = Convert.ToInt32(paras[paras.Count - 2].Value);
            int newCurrentPage = Convert.ToInt32(paras[paras.Count - 1].Value);
            return list.ToPagedList(newCurrentPage, pageSize, totalItemCount);
        }
        #endregion

        #region  查询掉线
        public static AsiatekPagedList<LostReportListModel> GetLostPagedSpeed(LostReportModels model, int searchPage, int pageSize, int userID)
        {
            List<SqlParameter> paras = new List<SqlParameter>()
            {
                
                new SqlParameter("@tableName","Vehicles v"),
                new SqlParameter("@joinStr",@"INNER JOIN VehiclesTerminals vt on v.ID=vt.VehicleID
INNER JOIN Terminals t on vt.TerminalID=t.ID 
INNER JOIN Exception_3 E ON E.TerminalCode=T.TerminalCode 
INNER JOIN Structures S ON S.ID=v.StrucID"),
                new SqlParameter("@pageSize",pageSize),
                new SqlParameter("@currentPage",searchPage),
                new SqlParameter("@orderBy","E.ID"),
                new SqlParameter("@showColumns",@"s.StrucName,v.VehicleName,e.GPSStartTime,e.GPSEndTime,
CAST(DATEDIFF(SS,GPSStartTime,GPSEndTime) AS VARCHAR) AS TIME"),
            };

            //string conditionStr = " E.[Status]<>9 AND E.[Status]<>8 ";//不查询删除和报废的
            string conditionStr = " vt.IsPrimary=1  AND v.Status=0 ";//主终端

            #region   查询参数
            if (string.IsNullOrWhiteSpace(model.VehicleName))
            {
                conditionStr += @" AND v.VehicleName IN (
SELECT vt.VehicleName FROM dbo.Structures s INNER JOIN
(
SELECT v.VehicleName,v.StrucID,V.ID AS VID FROM dbo.Vehicles v 
INNER JOIN
(SELECT StrucID FROM dbo.StructureDistributionInfo
WHERE UserID=" + userID + @") AS temp1 ON v.StrucID=temp1.StrucID
WHERE v.Status=0 AND v.IsReceived=1  
UNION 
SELECT v.VehicleName,v.StrucID,v.ID AS VID FROM dbo.Vehicles v 
INNER JOIN
(SELECT VehicleID FROM dbo.VehicleDistributionInfo 
WHERE UserID=" + userID + @") AS temp1 ON v.ID=temp1.VehicleID
WHERE v.Status=0 AND v.IsReceived=1 
) AS vt ON s.ID=vt.StrucID)";
            }
            if (!string.IsNullOrWhiteSpace(model.VehicleName))
            {
                conditionStr += " AND v.VehicleName LIKE '%" + model.VehicleName + "%'";
            }
            if (!string.IsNullOrWhiteSpace(model.GPSStartTime))
            {
                conditionStr += " AND GPSStartTime>='" + model.GPSStartTime + "'";
                if (!string.IsNullOrWhiteSpace(model.GPSEndTime))
                {
                    conditionStr += " AND GPSEndTime<='" + model.GPSEndTime + "'";
                }
            }
            #endregion

            if (!string.IsNullOrWhiteSpace(conditionStr))
            {
                paras.Add(new SqlParameter("@conditionStr", conditionStr));
            }

            paras.Add(new SqlParameter()
            {
                ParameterName = "@totalItemCount",
                Direction = ParameterDirection.Output,
                SqlDbType = SqlDbType.Int
            });
            paras.Add(new SqlParameter()
            {
                ParameterName = "@newCurrentPage",
                Direction = ParameterDirection.Output,
                SqlDbType = SqlDbType.Int
            });
            List<LostReportListModel> list = ConvertToList<LostReportListModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.StoredProcedure, "Proc_GetPagedDatas", paras.ToArray()));
            int totalItemCount = Convert.ToInt32(paras[paras.Count - 2].Value);
            int newCurrentPage = Convert.ToInt32(paras[paras.Count - 1].Value);
            return list.ToPagedList(newCurrentPage, pageSize, totalItemCount);
        }
        #endregion

        #region 异常报表  含超速报表  疲劳驾驶报表 当天累计驾驶超时报表 超时停车报表

        #region 自由模式
        /// <summary>
        /// 异常报表(自由模式)  含超速报表  疲劳驾驶报表 当天累计驾驶超时报表 超时停车报表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<ExceptionModel> GetExceptions(ExceptionSearchModel model)
        {
            string sql = @"SELECT e.ExceptionTypeID,e.EndAddress,e.StartAddress,e.ActualDuration,s.StrucName,uv.VehicleName,
                                    e.ServerStartTime AS StartDateTime,e.ServerEndTime AS EndDateTime";

            int exceptionTypeID = model.ExceptionTypeID;
            if (exceptionTypeID == (int)ExceptionTypeEnum.OverSpeedRpt)
            {
                sql += ",MaxSpeed,OverspeedThreshold,MinimumDuration ";
            }
            else if (exceptionTypeID == (int)ExceptionTypeEnum.FatigueDrivingRpt)
            {
                sql += ",ContinuousDrivingThreshold,MinimumBreakTime ";
            }
            else if (exceptionTypeID == (int)ExceptionTypeEnum.AccumulatedDrivingOvertime)
            {
                sql += ",DrivingTimeThreshold ";
            }
            else if (exceptionTypeID == (int)ExceptionTypeEnum.OvertimeParking)
            {
                sql += ",MaximumParkingTime,ParkingTime ";
            }

            sql += @" FROM dbo.VW_Exceptions AS e INNER JOIN Func_GetVehiclesListByUserID_New(@UserID) AS uv ON e.VIN = uv.VIN
                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                      WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                       OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) AND e.ExceptionTypeID = @ExceptionTypeID";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                new SqlParameter("@ExceptionTypeID",SqlDbType.Int)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            paras[4].Value = exceptionTypeID;
            return ConvertToList<ExceptionModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #region 默认模式
        /// <summary>
        /// 异常报表(默认模式)  含超速报表  疲劳驾驶报表 当天累计驾驶超时报表 超时停车报表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<ExceptionModel> GetDefaultExceptions(ExceptionSearchModel model, int strucID)
        {
            string sql = @"SELECT e.ExceptionTypeID,e.EndAddress,e.StartAddress,e.ActualDuration,s.StrucName,uv.VehicleName,
                                    e.ServerStartTime AS StartDateTime,e.ServerEndTime AS EndDateTime";

            int exceptionTypeID = model.ExceptionTypeID;
            if (exceptionTypeID == (int)ExceptionTypeEnum.OverSpeedRpt)
            {
                sql += ",MaxSpeed,OverspeedThreshold,MinimumDuration ";
            }
            else if (exceptionTypeID == (int)ExceptionTypeEnum.FatigueDrivingRpt)
            {
                sql += ",ContinuousDrivingThreshold,MinimumBreakTime ";
            }
            else if (exceptionTypeID == (int)ExceptionTypeEnum.AccumulatedDrivingOvertime)
            {
                sql += ",DrivingTimeThreshold ";
            }
            else if (exceptionTypeID == (int)ExceptionTypeEnum.OvertimeParking)
            {
                sql += ",MaximumParkingTime,ParkingTime ";
            }

            sql += @" FROM dbo.VW_Exceptions AS e INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv ON e.VIN = uv.VIN
                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                      WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                       OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) AND e.ExceptionTypeID = @ExceptionTypeID";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                new SqlParameter("@ExceptionTypeID",SqlDbType.Int)
            };
            paras[0].Value = strucID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            paras[4].Value = exceptionTypeID;
            return ConvertToList<ExceptionModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #endregion

        #region 异常报表  含紧急报警报表

        #region 自由模式
        /// <summary>
        /// 异常报表 （自由模式） 含紧急报警报表 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<ExceptionsAndDealInfoModel> GetExceptionsAndDealInfo(ExceptionSearchModel model)
        {
            string sql = @"SELECT e.ExceptionTypeID,e.EndAddress,e.StartAddress,e.ActualDuration,s.StrucName,uv.VehicleName,
                                    e.ServerStartTime AS StartDateTime,e.ServerEndTime AS EndDateTime";

            int exceptionTypeID = model.ExceptionTypeID;
            if (exceptionTypeID == (int)ExceptionTypeEnum.EmergencyAlarmRpt)
            {
                sql += " ,e.[DealTime],e.[DealInfo],u.NickName AS DealUser,u.UserName ";
            }

            sql += @" FROM dbo.VW_Exceptions AS e INNER JOIN Func_GetVehiclesListByUserID_New(@UserID) AS uv ON e.VIN = uv.VIN
                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                       LEFT JOIN [Users] u ON e.DealUserID=u.ID
                                      WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                       OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) 
                                       AND e.ExceptionTypeID = @ExceptionTypeID";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID ";
            }
            if (!string.IsNullOrWhiteSpace(model.DealUserName))
            {
                sql += " AND ((u.UserName LIKE '%" + model.DealUserName + "%')  OR (u.NickName LIKE '%" + model.DealUserName + "%'))";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                new SqlParameter("@ExceptionTypeID",SqlDbType.Int),
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            paras[4].Value = exceptionTypeID;
            return ConvertToList<ExceptionsAndDealInfoModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #region 默认模式
        /// <summary>
        /// 异常报表（默认模式）  含紧急报警报表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<ExceptionsAndDealInfoModel> GetDefaultExceptionsAndDealInfo(ExceptionSearchModel model, int strucID)
        {
            string sql = @"SELECT e.ExceptionTypeID,e.EndAddress,e.StartAddress,e.ActualDuration,s.StrucName,uv.VehicleName,
                                    e.ServerStartTime AS StartDateTime,e.ServerEndTime AS EndDateTime";

            int exceptionTypeID = model.ExceptionTypeID;
            if (exceptionTypeID == (int)ExceptionTypeEnum.EmergencyAlarmRpt)
            {
                sql += " ,e.[DealTime],e.[DealInfo],u.NickName AS DealUser,u.UserName ";
            }

            sql += @" FROM dbo.VW_Exceptions AS e INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv ON e.VIN = uv.VIN
                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                       LEFT JOIN [Users] u ON e.DealUserID=u.ID
                                      WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                       OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) 
                                       AND e.ExceptionTypeID = @ExceptionTypeID";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID ";
            }
            if (!string.IsNullOrWhiteSpace(model.DealUserName))
            {
                sql += " AND ((u.UserName LIKE '%" + model.DealUserName + "%')  OR (u.NickName LIKE '%" + model.DealUserName + "%'))";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                new SqlParameter("@ExceptionTypeID",SqlDbType.Int),
            };
            paras[0].Value = strucID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            paras[4].Value = exceptionTypeID;
            return ConvertToList<ExceptionsAndDealInfoModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #endregion

        #region 异常报表  设备故障报表 电源异常报表

        #region 自由模式
        /// <summary>
        /// 异常报表（自由模式）  设备故障报表 电源异常报表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<ExceptionsEquipmentModel> GetExceptionsForEquipment(ExceptionSearchModel model)
        {
            string sql = @"SELECT e.ExceptionTypeID,e.EndAddress,e.StartAddress,e.ActualDuration,s.StrucName,uv.VehicleName,
                                    e.SignalStartTime AS StartDateTime,e.SignalEndTime AS EndDateTime,et.ExName AS ExTypeName 
                                    FROM dbo.VW_Exceptions AS e INNER JOIN Func_GetVehiclesListByUserID_New(@UserID) AS uv ON e.VIN = uv.VIN
                                    INNER JOIN dbo.ExceptionType AS et ON e.ExceptionTypeID = et.ID AND et.CollectID = @CollectID
                                    INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                    WHERE ((e.SignalStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    OR (e.SignalEndTime BETWEEN @BeginDateTime AND @EndDateTime)) ";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID ";
            }
            if (model.ExceptionTypeID > 0)
            {
                sql += " AND e.ExceptionTypeID = @ExceptionTypeID";
            }

            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                new SqlParameter("@ExceptionTypeID",SqlDbType.Int),
                new SqlParameter("@CollectID",SqlDbType.Int)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            paras[4].Value = model.ExceptionTypeID;
            paras[5].Value = model.CollectID;
            return ConvertToList<ExceptionsEquipmentModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #region 默认模式
        /// <summary>
        /// 异常报表（默认模式）  设备故障报表 电源异常报表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<ExceptionsEquipmentModel> GetDefaultExceptionsForEquipment(ExceptionSearchModel model, int strucID)
        {
            string sql = @"SELECT e.ExceptionTypeID,e.EndAddress,e.StartAddress,e.ActualDuration,s.StrucName,uv.VehicleName,
                                    e.SignalStartTime AS StartDateTime,e.SignalEndTime AS EndDateTime,et.ExName AS ExTypeName 
                                    FROM dbo.VW_Exceptions AS e INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv ON e.VIN = uv.VIN
                                    INNER JOIN dbo.ExceptionType AS et ON e.ExceptionTypeID = et.ID AND et.CollectID = @CollectID
                                    INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                    WHERE ((e.SignalStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    OR (e.SignalEndTime BETWEEN @BeginDateTime AND @EndDateTime)) ";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID ";
            }
            if (model.ExceptionTypeID > 0)
            {
                sql += " AND e.ExceptionTypeID = @ExceptionTypeID";
            }

            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                new SqlParameter("@ExceptionTypeID",SqlDbType.Int),
                new SqlParameter("@CollectID",SqlDbType.Int)
            };
            paras[0].Value = strucID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            paras[4].Value = model.ExceptionTypeID;
            paras[5].Value = model.CollectID;
            return ConvertToList<ExceptionsEquipmentModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #endregion

        #region 异常报表  异常明细汇总报表

        #region 自由模式
        /// <summary>
        /// 异常报表（自由模式）  设备故障报表 电源异常报表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<ExceptionsEquipmentModel> GetAllExceptions(ExceptionSearchModel model)
        {
            string sql = @"SELECT e.ExceptionTypeID,e.EndAddress,e.StartAddress,e.ActualDuration,s.StrucName,uv.VehicleName,
                                    e.ServerStartTime AS StartDateTime,e.ServerEndTime AS EndDateTime,et.ExName AS ExTypeName 
                                    FROM dbo.VW_Exceptions AS e INNER JOIN Func_GetVehiclesListByUserID_New(@UserID) AS uv ON e.VIN = uv.VIN
                                    INNER JOIN dbo.ExceptionType AS et ON e.ExceptionTypeID = et.ID
                                    INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                    WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) ";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID ";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            return ConvertToList<ExceptionsEquipmentModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #region 默认模式
        /// <summary>
        /// 异常报表（默认模式）  设备故障报表 电源异常报表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<ExceptionsEquipmentModel> GetDefaultAllExceptions(ExceptionSearchModel model, int strucID)
        {
            string sql = @"SELECT e.ExceptionTypeID,e.EndAddress,e.StartAddress,e.ActualDuration,s.StrucName,uv.VehicleName,
                                    e.ServerStartTime AS StartDateTime,e.ServerEndTime AS EndDateTime,et.ExName AS ExTypeName 
                                    FROM dbo.VW_Exceptions AS e INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv ON e.VIN = uv.VIN
                                    INNER JOIN dbo.ExceptionType AS et ON e.ExceptionTypeID = et.ID
                                    INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                    WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) ";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID ";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = strucID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            return ConvertToList<ExceptionsEquipmentModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #endregion

        #region  里程报表
        /// <summary>
        ///  里程报表
        /// </summary>
        /// <param name="model"></param>
        public static List<VehicleDistanceModel> GetVehicleDistance(ExceptionSearchModel model)
        {
            #region 构建SQL

            // 获取时间段  根据时间段查询对应的信号月表 月表的时间格式是yyyyMM
            string beginMonth = model.SartTime.ToString("yyyyMM");
            string endMonth = model.EndTime.ToString("yyyyMM");
            string sql = "SELECT si.VIN,si.SignalDateTime,si.Mileage AS Distance,s.StrucName,uv.VehicleName FROM ";
            if (beginMonth == endMonth)
            {
                sql += string.Format(@" (SELECT VIN,SignalDateTime,Mileage FROM GNSS.dbo.[{0}] WITH(NOLOCK)  
                                                       WHERE SignalDateTime BETWEEN @BeginDateTime AND @EndDateTime) AS  si ", beginMonth);
            }
            else
            {
                sql += string.Format(@" (SELECT VIN,SignalDateTime,Mileage FROM GNSS.dbo.[{0}] WITH(NOLOCK)  
                                                       WHERE SignalDateTime BETWEEN @BeginDateTime AND @EndDateTime UNION
                                                        SELECT VIN,SignalDateTime,Mileage FROM GNSS.dbo.[{1}] WITH(NOLOCK)  
                                                       WHERE SignalDateTime BETWEEN @BeginDateTime AND @EndDateTime) AS  si ", beginMonth, endMonth);
            }
            sql += @" INNER JOIN Func_GetVehiclesListByUserID_New(@UserID) AS uv ON si.VIN = uv.VIN
                             INNER JOIN Structures AS s ON uv.StrucID = s.ID";
            if (model.VehiclesID > 0)
            {
                sql += " WHERE uv.VID = @VehiclesID ";
            }
            sql += " ORDER BY si.VIN,si.Mileage";
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            var list = ConvertToList<VehicleDistanceModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
            #endregion

            #region 整合数据 得出里程
            List<VehicleDistanceModel> listResult = null;
            if (list != null && list.Count > 0)
            {
                listResult = new List<VehicleDistanceModel>();
                string vin = list.First().VIN;
                listResult.Add(GetDistance(list, vin));

                // 获取当前满足条件的最后一个索引值 就可以不需要重复遍历list里面的数据 只需要找出直接满足条件的数据即可
                // 同一个车架号 我们只关心最大的里程记录和最小里程记录对应的数据值
                int i = list.FindLastIndex(o => o.VIN == vin) + 1;// 这里加 1 取到的数据就是下一个VIN的数据  这样下面就可以直接进入到 if判断里面
                for (; i < list.Count; i++)
                {
                    if (vin != list[i].VIN)
                    {
                        vin = list[i].VIN;
                        listResult.Add(GetDistance(list, vin));
                        i = list.FindLastIndex(o => o.VIN == vin);//这里不需要加上 + 1，因为for循环本身会+1
                    }
                }
                //foreach (var item in list)
                //{
                //    if (vin != item.VIN)
                //    {
                //        vin = item.VIN;
                //        listResult.Add(GetDistance(list, vin));
                //    }
                //}
            }
            return listResult;
            #endregion

        }
        #endregion

        #region  里程报表

        #region 里程报表(自由模式)
        /// <summary>
        ///  里程报表
        /// </summary>
        /// <param name="model"></param>
        public static List<VehicleDistanceModel> GetVehicleDistance(ExceptionSearchModel model, string linkServerName)
        {
            // 自由模式的sql 里面没有REMOTE关键字
            #region 构建SQL
            // 获取时间段  根据时间段查询对应的信号月表 月表的时间格式是yyyyMM
            string beginMonth = model.SartTime.ToString("yyyyMM");
            string endMonth = model.EndTime.ToString("yyyyMM");
            string sql = string.Empty;
            string whereSql = string.Empty;
            if (model.VehiclesID > 0)
            {
                whereSql = " AND uv.VID = @VehiclesID ";
            }
            if (beginMonth == endMonth)
            {
                sql = @"SELECT si.VIN,si.SignalDateTime,si.Mileage AS Distance,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetVehiclesListByUserID_New(@UserID) AS uv 
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID ";
                sql += string.Format(@" INNER  JOIN [{0}].GNSS.dbo.[{1}]  AS  si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE SignalDateTime BETWEEN @BeginDateTime AND @EndDateTime  {2}", linkServerName, beginMonth, whereSql);
            }
            else
            {
                sql += string.Format(@" SELECT si.VIN,si.SignalDateTime,si.Mileage AS Distance,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetVehiclesListByUserID_New(@UserID) AS uv
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  INNER  JOIN [{0}].GNSS.dbo.[{1}]  AS si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE SignalDateTime >= @BeginDateTime {2}", linkServerName, beginMonth, whereSql);
                sql += " UNION ALL ";

                sql += string.Format(@" SELECT si.VIN,si.SignalDateTime,si.Mileage AS Distance,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetVehiclesListByUserID_New(@UserID) AS uv
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  INNER  JOIN [{0}].GNSS.dbo.[{1}]  AS si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE SignalDateTime <= @EndDateTime {2}", linkServerName, endMonth, whereSql);
            }
            // 去除SignalDateTime排序 数据库中建立了索引 默认是降序排列
            //sql += " ORDER BY si.VIN,si.SignalDateTime";
            //sql += " ORDER BY si.VIN";
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            var list = ConvertToList<VehicleDistanceModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
            //list.OrderBy(o => o.VIN).OrderByDescending(o => o.SignalDateTime);
            #endregion

            #region 整合数据 得出里程
            List<VehicleDistanceModel> listResult = null;
            if (list != null && list.Count > 0)
            {
                //List<VehicleDistanceModel> list = listSql.OrderBy(o => o.VIN).ThenByDescending(o => o.SignalDateTime).ToList<VehicleDistanceModel>();
                listResult = new List<VehicleDistanceModel>();
                string vin = list.First().VIN;
                listResult.Add(GetDistance(list, vin));

                // 获取当前满足条件的最后一个索引值 就可以不需要重复遍历list里面的数据 只需要找出直接满足条件的数据即可
                // 同一个车架号 我们只关心最大的里程记录和最小里程记录对应的数据值
                int i = list.FindLastIndex(o => o.VIN == vin) + 1;// 这里加 1 取到的数据就是下一个VIN的数据  这样下面就可以直接进入到 if判断里面
                for (; i < list.Count; i++)
                {
                    if (vin != list[i].VIN)
                    {
                        vin = list[i].VIN;
                        listResult.Add(GetDistance(list, vin));
                        i = list.FindLastIndex(o => o.VIN == vin);//这里不需要加上 + 1，因为for循环本身会+1
                    }
                }
            }

            return listResult;
            #endregion

        }
        #endregion

        #region 里程报表（默认模式）
        /// <summary>
        ///  里程报表（默认模式）
        /// </summary>
        /// <param name="model"></param>
        public static List<VehicleDistanceModel> GetDefaultVehicleDistance(ExceptionSearchModel model, string linkServerName, int strucID)
        {
            #region 构建SQL
            // 获取时间段  根据时间段查询对应的信号月表 月表的时间格式是yyyyMM
            string beginMonth = model.SartTime.ToString("yyyyMM");
            string endMonth = model.EndTime.ToString("yyyyMM");
            string sql = string.Empty;
            string whereSql = string.Empty;
            if (model.VehiclesID > 0)
            {
                whereSql = " AND uv.VID = @VehiclesID ";
            }
            if (beginMonth == endMonth)
            {
                sql = @"SELECT si.VIN,si.SignalDateTime,si.Mileage AS Distance,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv 
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID ";
                sql += string.Format(@" INNER REMOTE JOIN [{0}].GNSS.dbo.[{1}]  AS  si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE SignalDateTime BETWEEN @BeginDateTime AND @EndDateTime  {2}", linkServerName, beginMonth, whereSql);
            }
            else
            {
                sql += string.Format(@" SELECT si.VIN,si.SignalDateTime,si.Mileage AS Distance,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  INNER REMOTE JOIN [{0}].GNSS.dbo.[{1}]  AS si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE SignalDateTime >= @BeginDateTime {2}", linkServerName, beginMonth, whereSql);
                sql += " UNION ALL ";

                sql += string.Format(@" SELECT si.VIN,si.SignalDateTime,si.Mileage AS Distance,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  INNER REMOTE JOIN [{0}].GNSS.dbo.[{1}]  AS si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE SignalDateTime <= @EndDateTime {2}", linkServerName, endMonth, whereSql);
            }

            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = strucID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            var list = ConvertToList<VehicleDistanceModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
            #endregion

            #region 整合数据 得出里程
            List<VehicleDistanceModel> listResult = null;
            if (list != null && list.Count > 0)
            {
                //List<VehicleDistanceModel> list = listSql.OrderBy(o => o.VIN).ThenByDescending(o => o.SignalDateTime).ToList<VehicleDistanceModel>();
                listResult = new List<VehicleDistanceModel>();
                string vin = list.First().VIN;
                listResult.Add(GetDistance(list, vin));

                // 获取当前满足条件的最后一个索引值 就可以不需要重复遍历list里面的数据 只需要找出直接满足条件的数据即可
                // 同一个车架号 我们只关心最大的里程记录和最小里程记录对应的数据值
                int i = list.FindLastIndex(o => o.VIN == vin) + 1;// 这里加 1 取到的数据就是下一个VIN的数据  这样下面就可以直接进入到 if判断里面
                for (; i < list.Count; i++)
                {
                    if (vin != list[i].VIN)
                    {
                        vin = list[i].VIN;
                        listResult.Add(GetDistance(list, vin));
                        i = list.FindLastIndex(o => o.VIN == vin);//这里不需要加上 + 1，因为for循环本身会+1
                    }
                }
            }
            return listResult;
            #endregion

        }
        #endregion

        #endregion

        #region 里程报表 提高速度版本
        #region 里程报表（默认模式）
        /// <summary>
        ///  里程报表（默认模式）
        /// </summary>
        /// <param name="model"></param>
        public static List<VehicleDistanceModel> GetDefaultVehicleDistanceNew(ExceptionSearchModel model, string linkServerName, int strucID)
        {
            // 获取时间段  根据时间段查询对应的信号月表 月表的时间格式是yyyyMM
            string beginMonth = model.SartTime.ToString("yyyyMM");
            string endMonth = model.EndTime.ToString("yyyyMM");

            string whereSql = string.Empty;
            // 如果选择了车辆 则只需要查询当前车辆的信息
            if (model.VehiclesID > 0)
            {
                whereSql = string.Format(" WHERE uv.VID = {0} ", model.VehiclesID);
            }

            // 根据单位ID获取当前用户能查看到的所有车辆信息
            string vehiclesSql = string.Format(@"SELECT uv.VIN,s.StrucName,uv.VehicleName FROM Func_GetAllTheSubsetOfVehiclesByStrucID({0}) AS uv
                                                   INNER JOIN Structures AS s ON uv.StrucID = s.ID {1};", strucID, whereSql);
            DataTable dtVehicles = MSSQLHelper.ExecuteDataTable(CommandType.Text, vehiclesSql);

            // 这里构造皰sql语句比较长 所以使用StringBuilder
            StringBuilder minSql = new StringBuilder();
            StringBuilder maxSql = new StringBuilder();
            if (dtVehicles != null && dtVehicles.Rows.Count > 0)
            {

                if (beginMonth == endMonth)
                {
                    #region 同一个 月份
                    int dataCount = dtVehicles.Rows.Count;
                    for (int i = 0; i < dataCount; i++)
                    {
                        string vin = dtVehicles.Rows[i]["VIN"].ToString();
                        string strucName = dtVehicles.Rows[i]["StrucName"].ToString();
                        string vehicleName = dtVehicles.Rows[i]["VehicleName"].ToString();
                        string unionAll = " UNION ALL ";
                        if (i == dataCount - 1)
                        {
                            unionAll = string.Empty;
                        }

                        maxSql.AppendFormat(@" SELECT * FROM(SELECT TOP 1 si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE VIN ='{0}' AND si.SignalDateTime 
                                                                   BETWEEN '{5}' AND '{6}' ORDER BY si.SignalDateTime DESC) AS maxTemp  {7} ",
                                                                    vin, strucName, vehicleName, linkServerName, beginMonth, model.SartTime, model.EndTime, unionAll);
                        // 根据SignalDateTime升序排列 top1查询的是最小的记录
                        //                        minSql.AppendFormat(@" SELECT  si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                        //                                                                   '{2}' AS VehicleName FROM (
                        //                                                                   SELECT   VIN,SignalDateTime,Mileage,ROW_NUMBER() OVER (ORDER BY SignalDateTime ASC ) rid
                        //                                                                   FROM [{3}].GNSS.dbo.[{4}] WHERE  VIN ='{0}' AND SignalDateTime 
                        //                                                                   BETWEEN '{5}' AND '{6}') AS si WHERE   rid = 1 {7} ", vin, strucName, vehicleName, linkServerName,
                        //                                                                                                         beginMonth, model.SartTime, model.EndTime, unionAll);
                        minSql.AppendFormat(@"SELECT  TOP 1 si.VIN ,si.SignalDateTime ,si.Mileage AS Distance ,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE si.SignalDateTime =
                                                                  (SELECT MIN(SignalDateTime) FROM [{3}].GNSS.dbo.[{4}]  WHERE VIN = '{0}'  AND SignalDateTime BETWEEN '{5}' AND '{6}') 
                                                                  AND VIN = '{0}'  {7}", vin, strucName, vehicleName, linkServerName, beginMonth, model.SartTime, model.EndTime, unionAll);
                    }
                    #endregion
                }
                else
                {
                    #region 跨月份
                    int dataCount = dtVehicles.Rows.Count;
                    for (int i = 0; i < dataCount; i++)
                    {
                        string vin = dtVehicles.Rows[i]["VIN"].ToString();
                        string strucName = dtVehicles.Rows[i]["StrucName"].ToString();
                        string vehicleName = dtVehicles.Rows[i]["VehicleName"].ToString();
                        string unionAll = " UNION ALL ";
                        if (i == dataCount - 1)
                        {
                            unionAll = string.Empty;
                        }
                        maxSql.Append(@"SELECT * FROM(SELECT TOP 1 VIN,SignalDateTime, Distance, StrucName,VehicleName FROM (");
                        maxSql.AppendFormat(@" SELECT  si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE VIN ='{0}' AND si.SignalDateTime >='{5}' ",
                                                                      vin, strucName, vehicleName, linkServerName, beginMonth, model.SartTime);
                        maxSql.Append(" UNION ALL ");

                        maxSql.AppendFormat(@" SELECT  si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE VIN ='{0}' AND si.SignalDateTime <='{5}') 
                                                                    AS maxTemp ORDER BY maxTemp.SignalDateTime DESC) AS maxTotal {6}",
                                                                                                   vin, strucName, vehicleName, linkServerName, endMonth, model.EndTime, unionAll);


                        // 根据SignalDateTime升序排列 top1查询的是最小的记录
                        //                        minSql.AppendFormat(@" SELECT  si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                        //                                                                   '{2}' AS VehicleName FROM (
                        //                                                                   SELECT   VIN,SignalDateTime,Mileage,ROW_NUMBER() OVER (ORDER BY SignalDateTime ASC ) rid
                        //                                                                   FROM [{3}].GNSS.dbo.[{4}] WHERE  VIN ='{0}' AND SignalDateTime >='{5}') AS si WHERE   rid = 1", 
                        //                                                               vin, strucName, vehicleName, linkServerName,beginMonth, model.SartTime, model.EndTime);
                        //                        minSql.Append("  UNION ALL ");

                        //                        minSql.AppendFormat(@" SELECT  si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                        //                                                                   '{2}' AS VehicleName FROM (
                        //                                                                   SELECT   VIN,SignalDateTime,Mileage,ROW_NUMBER() OVER (ORDER BY SignalDateTime ASC ) rid
                        //                                                                   FROM [{3}].GNSS.dbo.[{4}] WHERE  VIN ='{0}' AND SignalDateTime <='{5}') AS si WHERE   rid = 1 {6}",
                        //                                                                       vin, strucName, vehicleName, linkServerName, endMonth, model.EndTime, unionAll);
                        minSql.Append(@"SELECT TOP 1 VIN,SignalDateTime, Distance, StrucName,VehicleName FROM (");
                        minSql.AppendFormat(@"SELECT  TOP 1 si.VIN ,si.SignalDateTime ,si.Mileage AS Distance ,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE si.SignalDateTime =
                                                                  (SELECT MIN(SignalDateTime) FROM [{3}].GNSS.dbo.[{4}]  WHERE VIN = '{0}'  AND SignalDateTime >= '{5}') 
                                                                  AND VIN = '{0}'", vin, strucName, vehicleName, linkServerName, beginMonth, model.SartTime);
                        minSql.Append("  UNION ALL ");
                        minSql.AppendFormat(@"SELECT  TOP 1 si.VIN ,si.SignalDateTime ,si.Mileage AS Distance ,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE si.SignalDateTime =
                                                                  (SELECT MIN(SignalDateTime) FROM [{3}].GNSS.dbo.[{4}]  WHERE VIN = '{0}'  AND SignalDateTime <= '{5}') 
                                                                  AND VIN = '{0}' ) AS minTemp {6}",
                                                                   vin, strucName, vehicleName, linkServerName, endMonth, model.EndTime, unionAll);



                    }
                    #endregion
                }
            }

            #region 里程构造
            // 这里是组合sql 因为一般情况下 数据的最大记录数和最小记录数是一样的
            // 两个sql查出来的数据默认车架号的排序方式是一样的 所以datatable中同一行所对应的车架号信息一样
            // 此处暂时不做数据处理 如果为了保险起见 可以把两个datatable转化成list 然后通过lambda表达式刷选
            DataSet dsData = MSSQLHelper.ExecuteDataSet(CommandType.Text, minSql.Append(maxSql).ToString());
            List<VehicleDistanceModel> listResult = new List<VehicleDistanceModel>();
            if (dsData != null && dsData.Tables.Count == 2)
            {
                DataTable dtMin = dsData.Tables[0];
                DataTable dtMax = dsData.Tables[1];
                if (dtMin != null && dtMin.Rows.Count > 0 && dtMax != null && dtMin.Rows.Count == dtMax.Rows.Count)
                {
                    for (int i = 0; i < dtMin.Rows.Count; i++)
                    {
                        VehicleDistanceModel vehicleModel = new VehicleDistanceModel();
                        vehicleModel.VehicleName = dtMin.Rows[i]["VehicleName"].ToString();
                        vehicleModel.StrucName = dtMin.Rows[i]["StrucName"].ToString();
                        vehicleModel.StartDateTime = Convert.ToDateTime(dtMin.Rows[i]["SignalDateTime"]);
                        vehicleModel.EndDateTime = Convert.ToDateTime(dtMax.Rows[i]["SignalDateTime"]);
                        vehicleModel.Distance = Math.Round(Convert.ToDouble(dtMax.Rows[i]["Distance"]) - Convert.ToDouble(dtMin.Rows[i]["Distance"]), 1);
                        listResult.Add(vehicleModel);
                    }
                }
            }
            #endregion

            return listResult;

        }
        #endregion

        #region 里程报表(自由模式)
        /// <summary>
        ///  里程报表(自由模式)
        /// </summary>
        /// <param name="model"></param>
        public static List<VehicleDistanceModel> GetVehicleDistanceNew(ExceptionSearchModel model, string linkServerName)
        {
            // 获取时间段  根据时间段查询对应的信号月表 月表的时间格式是yyyyMM
            string beginMonth = model.SartTime.ToString("yyyyMM");
            string endMonth = model.EndTime.ToString("yyyyMM");

            string whereSql = string.Empty;
            // 如果选择了车辆 则只需要查询当前车辆的信息
            if (model.VehiclesID > 0)
            {
                whereSql = string.Format(" WHERE uv.VID = {0} ", model.VehiclesID);
            }

            // 根据用户ID获取当前用户能查看到的所有车辆信息
            string vehiclesSql = string.Format(@"SELECT uv.VIN,s.StrucName,uv.VehicleName FROM Func_GetVehiclesListByUserID_New({0}) AS uv
                                                   INNER JOIN Structures AS s ON uv.StrucID = s.ID {1};", model.UserID, whereSql);
            DataTable dtVehicles = MSSQLHelper.ExecuteDataTable(CommandType.Text, vehiclesSql);

            // 这里构造皰sql语句比较长 所以使用StringBuilder
            StringBuilder minSql = new StringBuilder();
            StringBuilder maxSql = new StringBuilder();
            if (dtVehicles != null && dtVehicles.Rows.Count > 0)
            {

                if (beginMonth == endMonth)
                {
                    #region 同一个 月份
                    int dataCount = dtVehicles.Rows.Count;
                    for (int i = 0; i < dataCount; i++)
                    {
                        string vin = dtVehicles.Rows[i]["VIN"].ToString();
                        string strucName = dtVehicles.Rows[i]["StrucName"].ToString();
                        string vehicleName = dtVehicles.Rows[i]["VehicleName"].ToString();
                        string unionAll = " UNION ALL ";
                        if (i == dataCount - 1)
                        {
                            unionAll = string.Empty;
                        }
                        maxSql.AppendFormat(@" SELECT * FROM(SELECT TOP 1 si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE VIN ='{0}' AND si.SignalDateTime 
                                                                   BETWEEN '{5}' AND '{6}' ORDER BY si.SignalDateTime DESC) AS maxTemp  {7} ",
                                                                    vin, strucName, vehicleName, linkServerName, beginMonth, model.SartTime, model.EndTime, unionAll);
                        // 根据SignalDateTime升序排列 top1查询的是最小的记录
                        //                        minSql.AppendFormat(@" SELECT  si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                        //                                                                   '{2}' AS VehicleName FROM (
                        //                                                                   SELECT   VIN,SignalDateTime,Mileage,ROW_NUMBER() OVER (ORDER BY SignalDateTime ASC ) rid
                        //                                                                   FROM [{3}].GNSS.dbo.[{4}] WHERE  VIN ='{0}' AND SignalDateTime 
                        //                                                                   BETWEEN '{5}' AND '{6}') AS si WHERE   rid = 1 {7} ", vin, strucName, vehicleName, linkServerName,
                        //                                                                                                         beginMonth, model.SartTime, model.EndTime, unionAll);
                        minSql.AppendFormat(@"SELECT  TOP 1 si.VIN ,si.SignalDateTime ,si.Mileage AS Distance ,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE si.SignalDateTime =
                                                                  (SELECT MIN(SignalDateTime) FROM [{3}].GNSS.dbo.[{4}]  WHERE VIN = '{0}'  AND SignalDateTime BETWEEN '{5}' AND '{6}') 
                                                                  AND VIN = '{0}' {7}", vin, strucName, vehicleName, linkServerName, beginMonth, model.SartTime, model.EndTime, unionAll);

                    }
                    #endregion
                }
                else
                {
                    #region 跨月份
                    int dataCount = dtVehicles.Rows.Count;
                    for (int i = 0; i < dataCount; i++)
                    {
                        string vin = dtVehicles.Rows[i]["VIN"].ToString();
                        string strucName = dtVehicles.Rows[i]["StrucName"].ToString();
                        string vehicleName = dtVehicles.Rows[i]["VehicleName"].ToString();
                        string unionAll = " UNION ALL ";
                        if (i == dataCount - 1)
                        {
                            unionAll = string.Empty;
                        }
                        maxSql.Append(@"SELECT * FROM(SELECT TOP 1 VIN,SignalDateTime, Distance, StrucName,VehicleName FROM (");
                        maxSql.AppendFormat(@" SELECT  si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE VIN ='{0}' AND si.SignalDateTime >='{5}'",
                                                                      vin, strucName, vehicleName, linkServerName, beginMonth, model.SartTime);
                        maxSql.Append(" UNION ALL ");

                        maxSql.AppendFormat(@" SELECT  si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE VIN ='{0}' AND si.SignalDateTime <='{5}') 
                                                                    AS maxTemp ORDER BY maxTemp.SignalDateTime DESC) AS maxTotal {6}",
                                                                                                   vin, strucName, vehicleName, linkServerName, endMonth, model.EndTime, unionAll);

                        //                        // 根据SignalDateTime升序排列 top1查询的是最小的记录
                        //                        minSql.AppendFormat(@" SELECT  si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                        //                                                                   '{2}' AS VehicleName FROM (
                        //                                                                   SELECT   VIN,SignalDateTime,Mileage,ROW_NUMBER() OVER (ORDER BY SignalDateTime ASC ) rid
                        //                                                                   FROM [{3}].GNSS.dbo.[{4}] WHERE  VIN ='{0}' AND SignalDateTime >='{5}') AS si WHERE   rid = 1",
                        //                                                               vin, strucName, vehicleName, linkServerName, beginMonth, model.SartTime, model.EndTime);
                        //                        minSql.Append("  UNION ALL ");

                        //                        minSql.AppendFormat(@" SELECT  si.VIN,si.SignalDateTime,si.Mileage AS Distance,'{1}' AS StrucName,
                        //                                                                   '{2}' AS VehicleName FROM (
                        //                                                                   SELECT   VIN,SignalDateTime,Mileage,ROW_NUMBER() OVER (ORDER BY SignalDateTime ASC ) rid
                        //                                                                   FROM [{3}].GNSS.dbo.[{4}] WHERE  VIN ='{0}' AND SignalDateTime <='{5}') AS si WHERE   rid = 1 {6}",
                        //                                                                       vin, strucName, vehicleName, linkServerName, endMonth, model.EndTime, unionAll);

                        minSql.Append(@"SELECT TOP 1 VIN,SignalDateTime, Distance, StrucName,VehicleName FROM (");
                        minSql.AppendFormat(@"SELECT  TOP 1 si.VIN ,si.SignalDateTime ,si.Mileage AS Distance ,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE si.SignalDateTime =
                                                                  (SELECT MIN(SignalDateTime) FROM [{3}].GNSS.dbo.[{4}]  WHERE VIN = '{0}'  AND SignalDateTime >= '{5}') 
                                                                  AND VIN = '{0}'", vin, strucName, vehicleName, linkServerName, beginMonth, model.SartTime);
                        minSql.Append("  UNION ALL ");
                        minSql.AppendFormat(@"SELECT  TOP 1 si.VIN ,si.SignalDateTime ,si.Mileage AS Distance ,'{1}' AS StrucName,
                                                                   '{2}' AS VehicleName FROM [{3}].GNSS.dbo.[{4}] AS si WHERE si.SignalDateTime =
                                                                  (SELECT MIN(SignalDateTime) FROM [{3}].GNSS.dbo.[{4}]  WHERE VIN = '{0}'  AND SignalDateTime <= '{5}') 
                                                                  AND VIN = '{0}' ) AS minTemp {6}",
                                                                   vin, strucName, vehicleName, linkServerName, endMonth, model.EndTime, unionAll);


                    }
                    #endregion
                }
            }

            #region 里程构造
            // 这里是组合sql 因为一般情况下 数据的最大记录数和最小记录数是一样的
            // 两个sql查出来的数据默认车架号的排序方式是一样的 所以datatable中同一行所对应的车架号信息一样
            // 此处暂时不做数据处理 如果为了保险起见 可以把两个datatable转化成list 然后通过lambda表达式刷选
            DataSet dsData = MSSQLHelper.ExecuteDataSet(CommandType.Text, minSql.Append(maxSql).ToString());
            List<VehicleDistanceModel> listResult = new List<VehicleDistanceModel>();
            if (dsData != null && dsData.Tables.Count == 2)
            {
                DataTable dtMin = dsData.Tables[0];
                DataTable dtMax = dsData.Tables[1];
                if (dtMin != null && dtMin.Rows.Count > 0 && dtMax != null && dtMin.Rows.Count == dtMax.Rows.Count)
                {
                    for (int i = 0; i < dtMin.Rows.Count; i++)
                    {
                        VehicleDistanceModel vehicleModel = new VehicleDistanceModel();
                        vehicleModel.VehicleName = dtMin.Rows[i]["VehicleName"].ToString();
                        vehicleModel.StrucName = dtMin.Rows[i]["StrucName"].ToString();
                        vehicleModel.StartDateTime = Convert.ToDateTime(dtMin.Rows[i]["SignalDateTime"]);
                        vehicleModel.EndDateTime = Convert.ToDateTime(dtMax.Rows[i]["SignalDateTime"]);
                        vehicleModel.Distance = Math.Round(Convert.ToDouble(dtMax.Rows[i]["Distance"]) - Convert.ToDouble(dtMin.Rows[i]["Distance"]), 1);
                        listResult.Add(vehicleModel);
                    }
                }
            }
            #endregion

            return listResult;


        }
        #endregion
        #endregion

        #region  温度报表
        #region（自由模式）
        public static List<TemperModel> GetTemper(ExceptionSearchModel model, string linkServerName)
        {
            // 自由模式的sql 里面没有REMOTE关键字
            // 获取时间段  根据时间段查询对应的信号月表 月表的时间格式是yyyyMM
            string beginMonth = model.SartTime.ToString("yyyyMM");
            string endMonth = model.EndTime.ToString("yyyyMM");
            string sql = string.Empty;
            string whereSql = string.Empty;
            if (model.VehiclesID > 0)
            {
                whereSql = " AND uv.VID = @VehiclesID ";
            }
            if (beginMonth == endMonth)
            {
                sql = @"SELECT si.VIN,si.SignalDateTime,si.Temperature,si.ACCState,si.Speed,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetVehiclesListByUserID_New(@UserID) AS uv 
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID ";
                sql += string.Format(@" INNER  JOIN [{0}].GNSS.dbo.[{1}]  AS  si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE si.Temperature IS NOT NULL AND si.SignalDateTime BETWEEN @BeginDateTime AND @EndDateTime  {2}", linkServerName, beginMonth, whereSql);
            }
            else
            {
                sql += string.Format(@"SELECT si.VIN,si.SignalDateTime,si.Temperature,si.ACCState,si.Speed,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetVehiclesListByUserID_New(@UserID) AS uv
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  INNER  JOIN [{0}].GNSS.dbo.[{1}]  AS si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE si.Temperature IS NOT NULL AND si.SignalDateTime >= @BeginDateTime {2}", linkServerName, beginMonth, whereSql);
                sql += " UNION ALL ";

                sql += string.Format(@"SELECT si.VIN,si.SignalDateTime,si.Temperature,si.ACCState,si.Speed,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetVehiclesListByUserID_New(@UserID) AS uv
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  INNER  JOIN [{0}].GNSS.dbo.[{1}]  AS si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE si.Temperature IS NOT NULL AND si.SignalDateTime <= @EndDateTime {2}", linkServerName, endMonth, whereSql);
            }
            // 去除SignalDateTime排序 数据库中建立了索引 默认是降序排列
            //sql += " ORDER BY si.VIN,si.SignalDateTime";
            //sql += " ORDER BY si.VIN";
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            return ConvertToList<TemperModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #region（默认模式）
        public static List<TemperModel> GetDefaultTemper(ExceptionSearchModel model, string linkServerName, int strucID)
        {
            // 获取时间段  根据时间段查询对应的信号月表 月表的时间格式是yyyyMM
            string beginMonth = model.SartTime.ToString("yyyyMM");
            string endMonth = model.EndTime.ToString("yyyyMM");
            string sql = string.Empty;
            string whereSql = string.Empty;
            if (model.VehiclesID > 0)
            {
                whereSql = " AND uv.VID = @VehiclesID ";
            }
            if (beginMonth == endMonth)
            {
                sql = @"SELECT si.VIN,si.SignalDateTime,si.Temperature,si.ACCState,si.Speed,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv 
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID ";
                sql += string.Format(@" INNER REMOTE JOIN [{0}].GNSS.dbo.[{1}]  AS  si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE si.Temperature IS NOT NULL AND si.SignalDateTime BETWEEN @BeginDateTime AND @EndDateTime  {2}", linkServerName, beginMonth, whereSql);
            }
            else
            {
                sql += string.Format(@" SELECT si.VIN,si.SignalDateTime,si.Temperature,si.ACCState,si.Speed,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  INNER REMOTE JOIN [{0}].GNSS.dbo.[{1}]  AS si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE si.Temperature IS NOT NULL AND si.SignalDateTime >= @BeginDateTime {2}", linkServerName, beginMonth, whereSql);
                sql += " UNION ALL ";

                sql += string.Format(@" SELECT si.VIN,si.SignalDateTime,si.Temperature,si.ACCState,si.Speed,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  INNER REMOTE JOIN [{0}].GNSS.dbo.[{1}]  AS si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE si.Temperature IS NOT NULL AND si.SignalDateTime <= @EndDateTime {2}", linkServerName, endMonth, whereSql);
            }

            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = strucID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            return ConvertToList<TemperModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion
        #endregion

        #region  温度异常报表
        #region（自由模式）
        public static List<TemperExceptionModel> GetTemperException(ExceptionSearchModel model, string linkServerName)
        {
            string sql = @"SELECT s.StrucName,uv.VehicleName,e.ExceptionTypeID,e.InstallationPosition,e.LimitValue,e.HighestTemperature,
e.LowestTemperature,e.ServerStartTime as SignalStartTime,e.ServerEndTime as SignalEndTime,e.ActualDuration 
FROM dbo.VW_SysExceptions AS e INNER JOIN Func_GetVehiclesListByUserID_New(@UserID) AS uv ON e.VIN = uv.VIN
                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                      WHERE e.ExceptionTypeID IN (101,102) AND ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                       OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) ";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            return ConvertToList<TemperExceptionModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #region（默认模式）
        public static List<TemperExceptionModel> GetDefaultTemperException(ExceptionSearchModel model, string linkServerName, int strucID)
        {
            string sql = @"SELECT s.StrucName,uv.VehicleName,e.ExceptionTypeID,e.InstallationPosition,e.LimitValue,e.HighestTemperature,
e.LowestTemperature,e.ServerStartTime as SignalStartTime,e.ServerEndTime as SignalEndTime,e.ActualDuration 
FROM dbo.VW_SysExceptions AS e INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv ON e.VIN = uv.VIN
                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                      WHERE e.ExceptionTypeID IN (101,102) AND ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                       OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) ";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = strucID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            return ConvertToList<TemperExceptionModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion
        #endregion

        #region 离线率报表
        #region 自由模式
        public static List<OfflineModel> GetOffline(ExceptionSearchModel model, string linkServerName)
        {
            string sql = string.Empty;
            sql = @"SELECT si.VIN,si.SignalDateTime as LastSignalDateTime,s.StrucName,uv.VehicleName,(SELECT COUNT(*) FROM 
       Func_GetVehiclesListByUserID_New(@UserID) AS uv) AS VehcielNumber,datediff(day,si.SignalDateTime,@EndTime) AS OfflineDays FROM 
                                                        Func_GetVehiclesListByUserID_New(@UserID) AS uv 
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID ";
            sql += string.Format(@"INNER  JOIN [{0}].GNSS.dbo.[{1}]  AS  si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE datediff(day,si.SignalDateTime,@EndTime)>=30", linkServerName, "Signals");

            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                new SqlParameter("@EndTime",SqlDbType.DateTime)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.VehiclesID;
            paras[2].Value = DateTime.Now;
            return ConvertToList<OfflineModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }

        #endregion

        #region 默认模式
        public static List<OfflineModel> GetDefaultOffline(ExceptionSearchModel model, string linkServerName, int strucID)
        {
            string sql = string.Empty;
            sql = @"SELECT si.SignalDateTime as LastSignalDateTime,s.StrucName,uv.VehicleName,(SELECT COUNT(*) FROM 
       Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv) as VehicleNumber,datediff(day,si.SignalDateTime,@EndTime) as OfflineDays FROM 
                                                        Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv 
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID ";
            sql += string.Format(@" INNER REMOTE JOIN [{0}].GNSS.dbo.[{1}]  AS  si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        WHERE  datediff(day,si.SignalDateTime,@EndTime)>=30", linkServerName, "Signals");
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                 new SqlParameter("@EndTime",SqlDbType.DateTime)
            };
            paras[0].Value = strucID;
            paras[1].Value = model.VehiclesID;
            paras[2].Value = DateTime.Now;
            return ConvertToList<OfflineModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion
        #endregion

        #region 异常处理报表
        #region 自由模式
        public static List<ExceptionHandleModel> GetExceptionHandle(ExceptionSearchModel model, string linkServerName)
        {
            string sql = string.Empty;
            string whereSql = string.Empty;
            if (model.VehiclesID > 0)
            {
                whereSql = " AND uv.VID = @VehiclesID ";
            }
            //硬件自带的异常处理
            sql = @"SELECT et.ExName AS ExceptionType,si.ServerStartTime AS ExceptionBeginDateTime,si.EndAddress AS ExceptionEndAddress,si.ServerEndTime AS ExceptionEndDateTime,si.StartAddress AS ExceptionBeginAddress,u.UserName AS DealUserName, si.DealTime AS DealDateTime,si.DealInfo,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetVehiclesListByUserID_New(@UserID) AS uv 
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID ";
            sql += string.Format(@" INNER  JOIN vw_exceptions  AS  si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                        Inner Join Users u on u.ID=si.DealUserID 
                                                        Inner Join ExceptionType et on si.ExceptionTypeID=et.ID
                                                        WHERE si.Status=1 AND si.DealTime BETWEEN @BeginDateTime AND @EndDateTime  {0}", whereSql);
            sql += " UNION ALL ";
            //系统分析的异常处理
            sql += @"SELECT et.ExName AS ExceptionType,si.ServerStartTime AS ExceptionBeginDateTime,si.EndAddress AS ExceptionEndAddress,si.ServerEndTime AS ExceptionEndDateTime,si.StartAddress AS ExceptionBeginAddress,u.UserName AS DealUserName,si.DealTime AS DealDateTime,si.DealInfo,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetVehiclesListByUserID_New(@UserID) AS uv 
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID ";
            sql += string.Format(@" INNER  JOIN  vw_sysexceptions  AS  si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                        Inner Join Users u on u.ID=si.DealUserID 
                                        Inner Join ExceptionType et on si.ExceptionTypeID=et.ID
                                        WHERE si.Status=1 AND si.DealTime BETWEEN @BeginDateTime AND @EndDateTime  {0}", whereSql);
            // 去除SignalDateTime排序 数据库中建立了索引 默认是降序排列
            //sql += " ORDER BY si.VIN,si.SignalDateTime";
            //sql += " ORDER BY si.VIN";
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            return ConvertToList<ExceptionHandleModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #region 默认模式
        public static List<ExceptionHandleModel> GetDefaultExceptionHandle(ExceptionSearchModel model, string linkServerName, int strucID)
        {
            // 获取时间段  根据时间段查询对应的信号月表 月表的时间格式是yyyyMM
            string sql = string.Empty;
            string whereSql = string.Empty;
            if (model.VehiclesID > 0)
            {
                whereSql = " AND uv.VID = @VehiclesID ";
            }
            //硬件异常处理
            sql = @"SELECT et.ExName AS ExceptionType,si.ServerStartTime AS ExceptionBeginDateTime,si.ServerEndTime AS ExceptionEndDateTime,si.EndAddress AS ExceptionEndAddress,si.StartAddress AS ExceptionBeginAddress,u.UserName AS DealUserName, si.DealTime AS DealDateTime,si.DealInfo,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv 
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID ";
            sql += string.Format(@" INNER  JOIN vw_exceptions  AS  si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                                Inner Join Users u on u.ID=si.DealUserID 
  Inner Join ExceptionType et on si.ExceptionTypeID=et.ID
                                                   WHERE si.Status=1 AND si.DealTime BETWEEN @BeginDateTime AND @EndDateTime    {0}", whereSql);
            sql += " UNION ALL ";
            //系统分析的异常处理
            sql += @"SELECT et.ExName AS ExceptionType,si.ServerStartTime AS ExceptionBeginDateTime,si.ServerEndTime AS ExceptionEndDateTime,si.EndAddress AS ExceptionEndAddress,si.StartAddress AS ExceptionBeginAddress,u.UserName AS DealUserName, si.DealTime AS DealDateTime,si.DealInfo,s.StrucName,uv.VehicleName FROM 
                                                        Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv 
                                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID ";
            sql += string.Format(@" INNER  JOIN  vw_sysexceptions  AS  si WITH(NOLOCK) ON si.VIN=uv.VIN 
                                        Inner Join Users u on u.ID=si.DealUserID 
  Inner Join ExceptionType et on si.ExceptionTypeID=et.ID
                                     WHERE si.Status=1 AND si.DealTime BETWEEN @BeginDateTime AND @EndDateTime   {0}", whereSql);
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                  new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = strucID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            return ConvertToList<ExceptionHandleModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }

        #endregion

        #endregion

        #region 夜间行驶报表
        #region 自由模式
        /// <summary>
        /// 异常报表(自由模式)  含超速报表  疲劳驾驶报表 当天累计驾驶超时报表 超时停车报表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<NightDrivingModel> GetNightDriving(ExceptionSearchModel model)
        {
            string sql = @"SELECT e.ExceptionTypeID,e.EndAddress,e.StartAddress,e.ActualDuration,s.StrucName,uv.VehicleName,
                                    e.ServerStartTime AS StartDateTime,e.ServerEndTime AS EndDateTime FROM dbo.VW_SysExceptions AS e  
                                      INNER JOIN Func_GetVehiclesListByUserID_New(@UserID) AS uv ON e.VIN = uv.VIN
                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                      WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                       OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) AND e.ExceptionTypeID = 103";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            return ConvertToList<NightDrivingModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion

        #region 默认模式
        /// <summary>
        /// 异常报表(默认模式)  含超速报表  疲劳驾驶报表 当天累计驾驶超时报表 超时停车报表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<NightDrivingModel> GetDefaultNightDriving(ExceptionSearchModel model, int strucID)
        {
            string sql = @"SELECT e.ExceptionTypeID,e.EndAddress,e.StartAddress,e.ActualDuration,s.StrucName,uv.VehicleName,
                                    e.ServerStartTime AS StartDateTime,e.ServerEndTime AS EndDateTime FROM dbo.VW_SysExceptions AS e 
                                    INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv ON e.VIN = uv.VIN
                                       INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                      WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                       OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) AND e.ExceptionTypeID = 103";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
            };
            paras[0].Value = strucID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            return ConvertToList<NightDrivingModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion
        #endregion

        #region 南钢区域超速报表

        #region 南钢区域超速报表V1.0（已暂停使用）

        #region 默认模式
        //        /// <summary>
        //        ///  南钢区域超速报表(默认模式)
        //        /// </summary>
        //        /// <param name="model"></param>
        //        /// <param name="strucID"></param>
        //        /// <returns></returns>
        //        public static List<NGAreaOverSpeedModel> GetDefaultNGAreaOverSpeed(ExceptionSearchModel model, int strucID)
        //        {
        //            string sql = @"SELECT s.StrucName,uv.VehicleName,e.ServerStartTime AS StartDateTime,
        //                                     e.ServerEndTime AS EndDateTime ,e.MaxOverSpeed,e.ActualDuration,e.FenceName,e.LimitSpeedValue AS MaxSpeed
        //                                    FROM dbo.VW_ElectricFenceException AS e 
        //                                    INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv ON e.VIN = uv.VIN
        //                                    INNER JOIN Structures AS s ON uv.StrucID = s.ID  
        //                                    WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
        //                                    OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) AND e.ExceptionType = 2
        //                                    AND e.ServerEndTime IS NOT NULL";
        //            if (model.VehiclesID > 0)
        //            {
        //                sql += " AND uv.VID = @VehiclesID";
        //            }
        //            List<SqlParameter> paras = new List<SqlParameter>() 
        //            { 
        //                new SqlParameter("@StrucID",SqlDbType.Int),
        //                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
        //                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
        //                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
        //            };
        //            paras[0].Value = strucID;
        //            paras[1].Value = model.SartTime;
        //            paras[2].Value = model.EndTime;
        //            paras[3].Value = model.VehiclesID;
        //            var result = ConvertToList<NGAreaOverSpeedModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        //            return GetNGAreaOverSpeed(result);
        //        }
        #endregion

        #region 自由模式
        //        /// <summary>
        //        /// 南钢区域超速报表(自由模式)
        //        /// </summary>
        //        /// <param name="model"></param>
        //        /// <returns></returns>
        //        public static List<NGAreaOverSpeedModel> GetNGAreaOverSpeed(ExceptionSearchModel model)
        //        {
        //            string sql = @"SELECT s.StrucName,uv.VehicleName,e.ServerStartTime AS StartDateTime,
        //                                     e.ServerEndTime AS EndDateTime ,e.MaxOverSpeed,e.ActualDuration,e.FenceName,e.LimitSpeedValue AS MaxSpeed
        //                                    FROM dbo.VW_ElectricFenceException AS e 
        //                                    INNER JOIN Func_GetVehiclesListByUserID_New(@UserID) AS uv ON e.VIN = uv.VIN
        //                                    INNER JOIN Structures AS s ON uv.StrucID = s.ID  
        //                                    WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
        //                                    OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) AND e.ExceptionType = 2
        //                                    AND e.ServerEndTime IS NOT NULL";
        //            if (model.VehiclesID > 0)
        //            {
        //                sql += " AND uv.VID = @VehiclesID";
        //            }
        //            List<SqlParameter> paras = new List<SqlParameter>() 
        //            { 
        //                new SqlParameter("@UserID",SqlDbType.Int),
        //                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
        //                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
        //                new SqlParameter("@VehiclesID",SqlDbType.BigInt)
        //            };
        //            paras[0].Value = model.UserID;
        //            paras[1].Value = model.SartTime;
        //            paras[2].Value = model.EndTime;
        //            paras[3].Value = model.VehiclesID;
        //            var result = ConvertToList<NGAreaOverSpeedModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        //            return GetNGAreaOverSpeed(result);
        //        }
        #endregion

        #endregion

        #region 南钢区域超速报表V2.0
        #region 默认模式
        /// <summary>
        ///  南钢区域超速报表(默认模式)V2.0
        /// </summary>
        /// <param name="model"></param>
        /// <param name="strucID"></param>
        /// <param name="averageOfOverSpeed">超速平均值(km/h)</param>
        /// <param name="actualDuration">持续时间(s)</param>
        /// <returns></returns>
        public static List<NGAreaAverageOverSpeedModel> GetDefaultNGAreaAverageOverSpeed(ExceptionSearchModel model, int strucID, int averageOfOverSpeed, int actualDuration)
        {
            string sql = @"SELECT s.StrucName,uv.VehicleName,e.ServerStartTime AS StartDateTime,
                                     e.ServerEndTime AS EndDateTime ,e.AverageOverSpeed,e.ActualDuration,e.FenceName,e.LimitSpeedValue AS MaxSpeed
                                    FROM dbo.VW_ElectricFenceException AS e 
                                    INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS uv ON e.VIN = uv.VIN
                                    INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                    WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) AND e.ExceptionType = 2
                                    AND e.ServerEndTime IS NOT NULL AND e.AverageOverSpeed  IS NOT NULL";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@StrucID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                new SqlParameter("@AverageOverSpeed", SqlDbType.Int),
                new SqlParameter("@ActualDuration", SqlDbType.Int)

            };
            paras[0].Value = strucID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            paras[4].Value = 0;
            paras[5].Value = 0;

            var sp1 = new SqlParameter("@AverageOverSpeed", SqlDbType.Int);

            if (averageOfOverSpeed > 0)
            {
                sql += " AND e.AverageOverSpeed >= @AverageOverSpeed";
                paras[4].Value = averageOfOverSpeed;
            }
            if (actualDuration > 0)
            {
                sql += " AND e.ActualDuration >= @ActualDuration";
                paras[5].Value = actualDuration;
            }
            var result = ConvertToList<NGAreaAverageOverSpeedModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
            return GetNGAreaAverageOverSpeed(result);
        }
        #endregion

        #region 自由模式
        /// <summary>
        /// 南钢区域超速报表(自由模式)V2.0
        /// </summary>
        /// <param name="model"></param>
        /// <param name="averageOfOverSpeed">超速平均值(km/h)</param>
        /// <param name="actualDuration">持续时间(s)</param>
        /// <returns></returns>
        public static List<NGAreaAverageOverSpeedModel> GetDefaultNGAreaAverageOverSpeed(ExceptionSearchModel model, int averageOfOverSpeed, int actualDuration)
        {
            string sql = @"SELECT s.StrucName,uv.VehicleName,e.ServerStartTime AS StartDateTime,
                                     e.ServerEndTime AS EndDateTime ,e.AverageOverSpeed,e.ActualDuration,e.FenceName,e.LimitSpeedValue AS MaxSpeed
                                    FROM dbo.VW_ElectricFenceException AS e 
                                    INNER JOIN Func_GetVehiclesListByUserID_New(@UserID) AS uv ON e.VIN = uv.VIN
                                    INNER JOIN Structures AS s ON uv.StrucID = s.ID  
                                    WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) AND e.ExceptionType = 2
                                    AND e.ServerEndTime IS NOT NULL AND e.AverageOverSpeed IS NOT NULL";
            if (model.VehiclesID > 0)
            {
                sql += " AND uv.VID = @VehiclesID";
            }
            List<SqlParameter> paras = new List<SqlParameter>() 
            { 
                new SqlParameter("@UserID",SqlDbType.Int),
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                new SqlParameter("@AverageOverSpeed", SqlDbType.Int),
                new SqlParameter("@ActualDuration", SqlDbType.Int)
            };
            paras[0].Value = model.UserID;
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;
            paras[4].Value = 0;
            paras[5].Value = 0;
            var sp1 = new SqlParameter("@AverageOverSpeed", SqlDbType.Int);

            if (averageOfOverSpeed > 0)
            {
                sql += " AND e.AverageOverSpeed >= @AverageOverSpeed";
                paras[4].Value = averageOfOverSpeed;
            }
            if (actualDuration > 0)
            {
                sql += " AND e.ActualDuration >= @ActualDuration";
                paras[5].Value = actualDuration;
            }
            var result = ConvertToList<NGAreaAverageOverSpeedModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
            return GetNGAreaAverageOverSpeed(result);
        }
        #endregion
        #endregion

        #endregion

        #region 马钢嘉华卸料异常报表
        /// <summary>
        /// 获取马钢嘉华卸料异常报表数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<MGJHExpForIllegalDischargModel> GetMGJHExpForIllegalDischarg(ExceptionSearchModel model, int strucID = 0)
        {

            List<SqlParameter> paras = new List<SqlParameter>() 
                { 
                    new SqlParameter("@StrucID",SqlDbType.Int),
                    new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                    new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                    new SqlParameter("@VehiclesID",SqlDbType.BigInt)
                };
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID;

            string sql;
            if (strucID > 0)//默认模式
            {
                paras[0].Value = strucID;
                sql = @"SELECT psd.PoundSheetCode,psd.CustomerName, psd.ShippingAddress,v.VehicleName,s.StrucName,
                                    e.ServerStartTime,e.ServerEndTime,e.ActualDuration	FROM dbo.VW_MGJH_SysExceptions e 
                                    INNER JOIN 	CustomerDataDBWebServer.CustomerDataDB.dbo.MGJH_PoundSheetDatas psd 
                                    ON e.PoundSheetID=psd.ID 
                                    INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS v ON v.VIN=e.VIN
                                    INNER JOIN dbo.Structures s ON s.ID=v.StrucID 
                                    WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) AND e.ExceptionTypeID = 104
                                    AND e.ServerEndTime IS NOT NULL";
            }
            else//自由模式
            {
                paras[0].Value = model.UserID;
                sql = @"SELECT psd.PoundSheetCode,psd.CustomerName, psd.ShippingAddress,v.VehicleName,s.StrucName,
                                    e.ServerStartTime,e.ServerEndTime,e.ActualDuration	FROM dbo.VW_MGJH_SysExceptions e 
                                    INNER JOIN 	CustomerDataDBWebServer.CustomerDataDB.dbo.MGJH_PoundSheetDatas psd 
                                    ON e.PoundSheetID=psd.ID 
                                    INNER JOIN Func_GetVehiclesListByUserID_New(@StrucID) AS v ON v.VIN=e.VIN
                                    INNER JOIN dbo.Structures s ON s.ID=v.StrucID 
                                    WHERE ((e.ServerStartTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    OR (e.ServerEndTime BETWEEN @BeginDateTime AND @EndDateTime)) AND e.ExceptionTypeID = 104
                                    AND e.ServerEndTime IS NOT NULL";
            }


            if (model.VehiclesID > 0)
            {
                sql += " AND v.VID = @VehiclesID";
            }

            return ConvertToList<MGJHExpForIllegalDischargModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));

        }
        #endregion

        #region 马钢嘉华营运报表
        /// <summary>
        /// 获取马钢嘉华营运报表
        /// </summary>
        /// <param name="model"></param>
        /// <param name="transportDuration">运送时长（分钟)</param>
        /// <param name="stayDuration">停留时长（分钟)</param>
        /// <param name="strucID"></param>
        /// <returns></returns>
        public static List<MGJHServiceModel> GetMGJHService(ExceptionSearchModel model,string viewMode="0", int transportDuration = 0, int stayDuration = 0, int strucID = 0)
        {
            List<SqlParameter> paras = new List<SqlParameter>() 
                { 
                    new SqlParameter("@StrucID",SqlDbType.Int),
                    new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                    new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                    new SqlParameter("@VehiclesID",SqlDbType.BigInt),
                    new SqlParameter("@TransportDuration", SqlDbType.Int),
                    new SqlParameter("@StayDuration", SqlDbType.Int)

                };
            paras[1].Value = model.SartTime;
            paras[2].Value = model.EndTime;
            paras[3].Value = model.VehiclesID; 
            paras[4].Value = model.VehiclesID;
            paras[5].Value = model.VehiclesID;


            string sql;
            if (strucID > 0)//默认模式
            {
                paras[0].Value = strucID;
                if (viewMode=="0")
                {//默认
                    sql = @"SELECT psd.PoundSheetCode ,
                                    psd.CustomerName ,
                                    psd.ShippingAddress ,
                                    psd.RealShippingAddressName ,
                                    psd.RealShippingAddressArea ,
                                    v.VehicleName ,
                                    s.StrucName ,
                                    psd.LeaveDeliveryPointTime ,
                                    psd.ReachUploadPointTime ,
                                    psd.TransportDuration/60 AS TransportDuration ,
                                    psd.LeaveUploadPointTime,
                                    psd.StayDuration/60 AS StayDuration,
                                    psd.CreateTime
                                    FROM CustomerDataDBWebServer.CustomerDataDB.dbo.MGJH_PoundSheetDatas psd
                                    INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS v ON v.VID=psd.VehicleID
									INNER JOIN dbo.Structures s ON s.ID=v.StrucID 
                                    WHERE (psd.CreateTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    AND psd.CreateTime IS NOT NULL 
                                    ORDER  BY psd.CreateTime DESC";
                }
                else
                {////不默认
                    sql = @"SELECT psd.PoundSheetCode ,
                                    psd.CustomerName ,
                                    psd.ShippingAddress ,
                                    psd.RealShippingAddressName ,
                                    psd.RealShippingAddressArea ,
                                    v.VehicleName ,
                                    s.StrucName ,
                                    psd.LeaveDeliveryPointTime ,
                                    psd.ReachUploadPointTime ,
                                    psd.TransportDuration/60 AS TransportDuration ,
                                    psd.LeaveUploadPointTime,
                                    psd.StayDuration/60 AS StayDuration,
                                    psd.CreateTime
                                    FROM CustomerDataDBWebServer.CustomerDataDB.dbo.MGJH_PoundSheetDatas psd
                                    INNER JOIN Func_GetAllTheSubsetOfVehiclesByStrucID(@StrucID) AS v ON v.PlateNum=psd.PlateNumber
									INNER JOIN dbo.Structures s ON s.ID=v.StrucID 
                                    WHERE (psd.CreateTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    AND psd.CreateTime IS NOT NULL 
                                    ORDER  BY psd.CreateTime DESC";
                }

                
            }
            else//自由模式
            {
                paras[0].Value = model.UserID;
                if (viewMode == "0")//默认
                {
                    sql = @"SELECT psd.PoundSheetCode ,
                                    psd.CustomerName ,
                                    psd.ShippingAddress ,
                                    psd.RealShippingAddressName ,
                                    psd.RealShippingAddressArea ,
                                    v.VehicleName ,
                                    s.StrucName ,
                                    psd.LeaveDeliveryPointTime ,
                                    psd.ReachUploadPointTime ,
                                    psd.TransportDuration/60 AS TransportDuration ,
                                    psd.LeaveUploadPointTime,
                                    psd.StayDuration/60 AS StayDuration,
                                    psd.CreateTime
                                    FROM CustomerDataDBWebServer.CustomerDataDB.dbo.MGJH_PoundSheetDatas psd
                                    INNER JOIN Func_GetVehiclesListByUserID_New(@StrucID) AS v ON v.VID=psd.VehicleID
									INNER JOIN dbo.Structures s ON s.ID=v.StrucID 
                                    WHERE (psd.CreateTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    AND psd.CreateTime IS NOT NULL 
                                    ORDER  BY psd.CreateTime DESC";
                }
                else//不默认
                {
                    sql = @"SELECT psd.PoundSheetCode ,
                                    psd.CustomerName ,
                                    psd.ShippingAddress ,
                                    psd.RealShippingAddressName ,
                                    psd.RealShippingAddressArea ,
                                    v.VehicleName ,
                                    s.StrucName ,
                                    psd.LeaveDeliveryPointTime ,
                                    psd.ReachUploadPointTime ,
                                    psd.TransportDuration/60 AS TransportDuration ,
                                    psd.LeaveUploadPointTime,
                                    psd.StayDuration/60 AS StayDuration,
                                    psd.CreateTime
                                    FROM CustomerDataDBWebServer.CustomerDataDB.dbo.MGJH_PoundSheetDatas psd
                                    INNER JOIN Func_GetVehiclesListByUserID_New(@StrucID) AS v ON v.PlateNum=psd.PlateNumber
									INNER JOIN dbo.Structures s ON s.ID=v.StrucID 
                                    WHERE (psd.CreateTime BETWEEN @BeginDateTime AND @EndDateTime) 
                                    AND psd.CreateTime IS NOT NULL 
                                    ORDER  BY psd.CreateTime DESC";
                }
                
            }


            if (model.VehiclesID > 0)
            {
                sql += " AND v.VID = @VehiclesID";
            }
            if (transportDuration > 0)
            {
                paras[4].Value = transportDuration;
                sql += " AND psd.TransportDuration>=(@TransportDuration * 60 )";
            }
            if (stayDuration > 0)
            {
                paras[5].Value = stayDuration;
                sql += " AND psd.StayDuration<=(@StayDuration * 60)";
            }

            return ConvertToList<MGJHServiceModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql, paras.ToArray()));
        }
        #endregion


        #region 分组获取对应的里程
        private static VehicleDistanceModel GetDistance(List<VehicleDistanceModel> list, string vin)
        {
            VehicleDistanceModel modelFirst = list.First(o => (o.VIN == vin));
            VehicleDistanceModel modelLast = list.Last(o => (o.VIN == vin));
            return new VehicleDistanceModel()
            {
                //Distance = Math.Round(modelLast.Distance - modelFirst.Distance, 1),
                //EndDateTime = modelLast.SignalDateTime,
                //StartDateTime = modelFirst.SignalDateTime,
                Distance = Math.Round(modelFirst.Distance - modelLast.Distance, 1),
                EndDateTime = modelFirst.SignalDateTime,
                StartDateTime = modelLast.SignalDateTime,
                VehicleName = modelFirst.VehicleName,
                StrucName = modelFirst.StrucName
            };
        }
        #endregion

        #region 获取连接服务器信息

        #region 自由模式
        /// <summary>
        /// 获取连接服务器信息(自由模式)
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="vehiclesID"></param>
        /// <returns></returns>
        public static List<ServerInfoModel> GetServerInfo(int userID, long vehiclesID)
        {
            // 如果 VehiclesID > 0  则选择了车辆代号查询 那么此时只需要查询一个链接服务器即可 ，
            // 如果不是 则查询当前用户能查询的车辆下所关联的所有链接服务器
            string vehiclesIDSql = string.Empty;
            #region 注释代码
            //            if (vehiclesID > 0)
            //            {
            //                vehiclesIDSql = " AND v.ID=" + vehiclesID;
            //            }
            //            string serverInfoSql = string.Format(@" SELECT DISTINCT( s.LinkedServerName ) FROM (
            //                                                                    SELECT  v.ID FROM    Vehicles v INNER JOIN ( SELECT  *
            //                                                                    FROM   StructureDistributionInfo WHERE  UserID = {0}
            //                                                                  )  s ON v.StrucID = s.StrucID WHERE   v.[Status] = 0 AND v.IsReceived = 1 {1}
            //                                                                    UNION
            //                                                                 SELECT    v.ID FROM   Vehicles v INNER JOIN ( SELECT *
            //                                                                 FROM   VehicleDistributionInfo WHERE  UserID = {0}
            //                                                                 ) s ON v.ID = s.VehicleID WHERE    v.[Status] = 0 AND v.IsReceived = 1 {1}) AS UserVehicles
            //                                                                 INNER JOIN Terminals AS t ON UserVehicles.ID = t.LinkedVehicleID
            //                                                                 INNER JOIN dbo.ServerInfo AS s ON t.ServerInfoID = s.ID;", userID, vehiclesIDSql);

            #endregion

            if (vehiclesID > 0)
            {
                vehiclesIDSql = " WHERE UserVehicles.VehicleID=" + vehiclesID;
            }
            string serverInfoSql = string.Format(@" SELECT DISTINCT( s.LinkedServerName ) 
                                                                 FROM Func_New_GetVehicleIDByUserId({0}) AS UserVehicles
                                                                 INNER JOIN dbo.ServerInfo AS s ON UserVehicles.ServerInfoID = s.ID {1};", userID, vehiclesIDSql);
            return ConvertToList<ServerInfoModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, serverInfoSql, null));
        }
        #endregion

        #region 默认模式
        /// <summary>
        /// 获取连接服务器信息(默认模式)
        /// </summary>
        /// <param name="strucID"></param>
        /// <param name="vehiclesID"></param>
        /// <returns></returns>
        public static List<ServerInfoModel> GetDefaultServerInfo(int strucID, long vehiclesID)
        {
            // 如果 VehiclesID > 0  则选择了车辆代号查询 那么此时只需要查询一个链接服务器即可 ，
            // 如果不是 则查询当前用户能查询的车辆下所关联的所有链接服务器
            string vehiclesIDSql = string.Empty;
            if (vehiclesID > 0)
            {
                vehiclesIDSql = " WHERE UserVehicles.VehicleID=" + vehiclesID;
            }
            string serverInfoSql = string.Format(@" SELECT DISTINCT( s.LinkedServerName ) 
                                                                 FROM Func_GetVehicleIDByStrucID({0}) AS UserVehicles
                                                                 INNER JOIN dbo.ServerInfo AS s ON UserVehicles.ServerInfoID = s.ID {1};", strucID, vehiclesIDSql);

            return ConvertToList<ServerInfoModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, serverInfoSql, null));
        }
        #endregion

        #endregion

        #region 报表处理

        #region 最大超速Model转换(暂停使用)
        ///// <summary>
        ///// 最大超速Model转换
        ///// </summary>
        ///// <param name="list"></param>
        ///// <returns></returns>
        //public static List<NGAreaOverSpeedModel> GetNGAreaOverSpeed(List<NGAreaOverSpeedModel> list)
        //{
        //    List<NGAreaOverSpeedModel> result = new List<NGAreaOverSpeedModel>();
        //    foreach (var item in list)
        //    {
        //        NGAreaOverSpeedModel model = new NGAreaOverSpeedModel();
        //        model.StrucName = item.StrucName;
        //        model.FenceName = item.FenceName;
        //        model.VehicleName = item.VehicleName;
        //        model.StartDateTime = item.StartDateTime;
        //        model.EndDateTime = item.EndDateTime;
        //        model.MaxSpeed = item.MaxSpeed;
        //        model.MaxOverSpeed = item.MaxOverSpeed;
        //        model.ActualDuration = item.ActualDuration;
        //        model.OverSpeedTimes = GetOverSpeedTimesByActualDuration(item.ActualDuration);
        //        model.Penalty = GetPenaltyBySpeed(item.MaxOverSpeed) * model.OverSpeedTimes;
        //        result.Add(model);
        //    }
        //    return result;
        //} 
        #endregion

        #region 平均超速Model转换
        /// <summary>
        /// 平均超速Model转换
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<NGAreaAverageOverSpeedModel> GetNGAreaAverageOverSpeed(List<NGAreaAverageOverSpeedModel> list)
        {
            List<NGAreaAverageOverSpeedModel> result = new List<NGAreaAverageOverSpeedModel>();
            foreach (var item in list)
            {
                NGAreaAverageOverSpeedModel model = new NGAreaAverageOverSpeedModel();
                model.StrucName = item.StrucName;
                model.FenceName = item.FenceName;
                model.VehicleName = item.VehicleName;
                model.StartDateTime = item.StartDateTime;
                model.EndDateTime = item.EndDateTime;
                model.MaxSpeed = item.MaxSpeed;
                model.AverageOverSpeed = item.AverageOverSpeed;
                model.ActualDuration = item.ActualDuration;
                model.OverSpeedTimes = GetOverSpeedTimesByActualDuration(item.ActualDuration);
                model.Penalty = GetPenaltyBySpeed(item.AverageOverSpeed) * model.OverSpeedTimes;
                model.OverSpeedPercent = GetOverSpeedPercent(item.AverageOverSpeed, item.MaxSpeed);
                result.Add(model);
            }
            return result;
        }
        #endregion

        #region 根据速度获取罚款金额
        /// <summary>
        /// 根据速度获取罚款金额
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static double GetPenaltyBySpeed(double speed)
        {
            double penalty;
            if (35 <= speed && speed < 40)
            {
                penalty = 50;
            }
            else if (40 <= speed && speed <= 45)
            {
                penalty = 100;
            }
            else if (speed > 45)
            {
                penalty = 200;
            }
            else
            {
                penalty = 0;
            }
            return penalty;
        }
        #endregion

        #region 根据持续时间获取超速次数
        /// <summary>
        /// 根据持续时间获取超速次数
        /// </summary>
        /// <param name="actualDuration"></param>
        /// <returns></returns>
        public static int GetOverSpeedTimesByActualDuration(double actualDuration)
        {
            // actualDuration<30 不算
            // 30<=actualDuration<60     *1
            // 60<=actualDuration<90      *2
            // 90<=actualDuration<120    *3
            // 120<=actualDuration<150    *4  以此类推
            return (int)actualDuration / 30;
        }
        #endregion

        #region 根据超速平均时速和限速值获取超速百分比
        /// <summary>
        /// 根据超速平均时速和限速值获取超速百分比
        /// </summary>
        /// <param name="averageOverSpeed">平均超速速度</param>
        /// <param name="maxSpeed">限速速度</param>
        /// <returns></returns>
        public static string GetOverSpeedPercent(double averageOverSpeed, double maxSpeed)
        {
            if (averageOverSpeed < 0 || maxSpeed <= 0)
            {
                return "0";
            }
            int result = (int)((averageOverSpeed - maxSpeed) / maxSpeed * 100);
            return result < 0 ? "0" : result + "%";
        }
        #endregion

        #endregion

        #region 门岗异常报表
        //电子围栏异常记录的是终端上的车辆标识，可能存在与平台车牌号不一致的情况，所以关联查询车牌号
        //门岗异常
        public static List<GateExceptionModel> GetGateException(GateExceptionSearchModel model, int timeInterval)
        {
            List<GateExceptionModel> rs = new List<GateExceptionModel>();

            #region 查询门岗数据和电子围栏数据
            // 查询符合搜索条件的门岗数据
            string whereSql_gate = string.Empty;
            if (!string.IsNullOrWhiteSpace(model.PlateNum))
            {
                whereSql_gate = " AND CarNumber = @PlateNum ";
            }
            string sql_gate = string.Format(@" SELECT CarNumber AS PlateNum,PassGate,InOrOut,InOrOutTime FROM dbo.GateSentryRecord 
                                     WHERE InOrOutTime BETWEEN @BeginDateTime AND @EndDateTime {0} AND SUBSTRING(CarNumber, 2,LEN((CarNumber))) IN 
(SELECT SUBSTRING(ve.PlateNum, 2,LEN((ve.PlateNum))) FROM dbo.VehicleElectricFence vef INNER JOIN dbo.Vehicles ve ON ve.ID=vef.VehicleID )  
 ORDER BY InOrOutTime ASC ", whereSql_gate);
            List<SqlParameter> paras_gate = new List<SqlParameter>() 
            { 
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@PlateNum",SqlDbType.NVarChar)
            };
            paras_gate[0].Value = model.SartTime;
            paras_gate[1].Value = model.EndTime;
            paras_gate[2].Value = FontHelper.StringConvert(model.PlateNum, "1");
            DataTable gateDt = MSSQLHelper.ExecuteDataTable(CommandType.Text, sql_gate, paras_gate.ToArray());

            //查询符合搜索条件的电子围栏数据
            string whereSql_ef = string.Empty;
            if (!string.IsNullOrWhiteSpace(model.PlateNum))
            {
                whereSql_ef = " AND ve.PlateNum = @PlateNum ";
            }
            string sql_ef = string.Format(@" SELECT ve.PlateNum,efe.ExceptionType,efe.SignalStartTime,efe.SignalEndTime  FROM  [dbo].[VW_ElectricFenceException] efe  
  INNER JOIN dbo.Terminals te ON te.TerminalCode = efe.TerminalCode 
  INNER JOIN dbo.Vehicles ve ON  ve.ID = te.LinkedVehicleID   
                                     WHERE efe.ExceptionType=0 AND (efe.SignalStartTime BETWEEN @BeginDateTime AND @EndDateTime OR efe.SignalEndTime BETWEEN @BeginDateTime AND @EndDateTime ) {0} 
 AND efe.SignalEndTime IS NOT NULL ORDER BY efe.SignalStartTime ASC ", whereSql_ef);
            List<SqlParameter> paras_ef = new List<SqlParameter>() 
            { 
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@PlateNum",SqlDbType.NVarChar)
            };
            paras_ef[0].Value = model.SartTime;
            paras_ef[1].Value = model.EndTime;
            paras_ef[2].Value = model.PlateNum;
            DataTable efDt = MSSQLHelper.ExecuteDataTable(CommandType.Text, sql_ef, paras_ef.ToArray());

            //查无数据
            if (gateDt.Rows.Count == 0)
            {
                return rs;
            }
            #endregion

            //数据对比的两种情况：
            //1、电子围栏异常结果集没有数据
            //2、有电子围栏异常的信息，但是和门岗信息时间间隔相差10（可配置）分钟以上

            DataTable tempDt = new DataTable();
            tempDt.Columns.Add("PlateNum", typeof(string));
            tempDt.Columns.Add("PassGate", typeof(string));
            tempDt.Columns.Add("InOrOut", typeof(string));
            tempDt.Columns.Add("InOrOutTime", typeof(string));//datetime类型不能为空，设置string类型

            #region 电子围栏异常结果集没有数据
            if (efDt.Rows.Count == 0)
            {
                for (int i = 0; i < gateDt.Rows.Count; i++)
                {
                    tempDt.Rows.Add(FontHelper.StringConvert(gateDt.Rows[i]["PlateNum"].ToString(), "2"), gateDt.Rows[i]["PassGate"], gateDt.Rows[i]["InOrOut"], gateDt.Rows[i]["InOrOutTime"]);
                }
            }
            #endregion

            #region 2、有电子围栏异常的信息
            else
            {
                //分析进出门岗信息，区别进入门岗和驶出门岗
                for (int i = 0; i < gateDt.Rows.Count; i++)
                {
                    string form = string.Empty;
                    string plateNum = FontHelper.StringConvert(gateDt.Rows[i]["PlateNum"].ToString(), "2");
                    //比较电子围栏开始时间
                    if (gateDt.Rows[i]["InOrOut"].Equals("入口"))
                    {
                        form = string.Format(" {0}='{1}' AND {2} >= '{3}' AND {4} <= '{5}' ", "PlateNum", plateNum,
                        "SignalStartTime", Convert.ToDateTime(gateDt.Rows[i]["InOrOutTime"]).AddMinutes(-timeInterval).ToString("yyyy-MM-dd HH:mm:ss"),
                        "SignalStartTime", Convert.ToDateTime(gateDt.Rows[i]["InOrOutTime"]).AddMinutes(timeInterval).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    //比较电子围栏结束时间
                    if (gateDt.Rows[i]["InOrOut"].Equals("出口"))
                    {
                        form = string.Format(" {0}='{1}' AND {2} >= '{3}' AND {4} <= '{5}' ", "PlateNum", plateNum,
                        "SignalEndTime", Convert.ToDateTime(gateDt.Rows[i]["InOrOutTime"]).AddMinutes(-timeInterval).ToString("yyyy-MM-dd HH:mm:ss"),
                        "SignalEndTime", Convert.ToDateTime(gateDt.Rows[i]["InOrOutTime"]).AddMinutes(timeInterval).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    //查不出记录算异常
                    if (efDt.Select(form).Count() == 0)
                    {
                        tempDt.Rows.Add(plateNum, gateDt.Rows[i]["PassGate"], gateDt.Rows[i]["InOrOut"], gateDt.Rows[i]["InOrOutTime"]);
                    }
                }
            }
            #endregion

            return ConvertToList<GateExceptionModel>.Convert(tempDt);
        }

        //电子围栏异常
        public static List<GateExceptionModel> GetEFException(GateExceptionSearchModel model, int timeInterval)
        {
            List<GateExceptionModel> rs = new List<GateExceptionModel>();

            #region 查询门岗数据和电子围栏数据
            // 查询符合搜索条件的门岗数据
            string whereSql_gate = string.Empty;
            if (!string.IsNullOrWhiteSpace(model.PlateNum))
            {
                whereSql_gate = " AND CarNumber = @PlateNum ";
            }
            string sql_gate = string.Format(@" SELECT CarNumber AS PlateNum,PassGate,InOrOut,InOrOutTime FROM dbo.GateSentryRecord 
                                     WHERE InOrOutTime BETWEEN @BeginDateTime AND @EndDateTime {0} AND SUBSTRING(CarNumber, 2,LEN((CarNumber))) IN 
(SELECT SUBSTRING(ve.PlateNum, 2,LEN((ve.PlateNum))) FROM dbo.VehicleElectricFence vef INNER JOIN dbo.Vehicles ve ON ve.ID=vef.VehicleID ) 
 ORDER BY InOrOutTime ASC ", whereSql_gate);
            List<SqlParameter> paras_gate = new List<SqlParameter>() 
            { 
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@PlateNum",SqlDbType.NVarChar)
            };
            paras_gate[0].Value = model.SartTime;
            paras_gate[1].Value = model.EndTime;
            paras_gate[2].Value = FontHelper.StringConvert(model.PlateNum, "1");
            DataTable gateDt = MSSQLHelper.ExecuteDataTable(CommandType.Text, sql_gate, paras_gate.ToArray());

            //查询符合搜索条件的电子围栏数据
            string whereSql_ef = string.Empty;
            if (!string.IsNullOrWhiteSpace(model.PlateNum))
            {
                whereSql_ef = " AND ve.PlateNum = @PlateNum ";
            }
            string sql_ef = string.Format(@" SELECT ve.PlateNum,efe.ExceptionType,efe.SignalStartTime,efe.SignalEndTime  FROM  [dbo].[VW_ElectricFenceException] efe  
  INNER JOIN dbo.Terminals te ON te.TerminalCode = efe.TerminalCode 
  INNER JOIN dbo.Vehicles ve ON  ve.ID = te.LinkedVehicleID   
                                     WHERE efe.ExceptionType=0 AND (efe.SignalStartTime BETWEEN @BeginDateTime AND @EndDateTime OR efe.SignalEndTime BETWEEN @BeginDateTime AND @EndDateTime ) {0}  
 AND efe.SignalEndTime IS NOT NULL  ORDER BY efe.SignalStartTime ASC ", whereSql_ef);
            List<SqlParameter> paras_ef = new List<SqlParameter>() 
            { 
                new SqlParameter("@BeginDateTime",SqlDbType.DateTime),
                new SqlParameter("@EndDateTime",SqlDbType.DateTime),
                new SqlParameter("@PlateNum",SqlDbType.NVarChar)
            };
            paras_ef[0].Value = model.SartTime;
            paras_ef[1].Value = model.EndTime;
            paras_ef[2].Value = model.PlateNum;
            DataTable efDt = MSSQLHelper.ExecuteDataTable(CommandType.Text, sql_ef, paras_ef.ToArray());

            //查无数据
            if (efDt.Rows.Count == 0)
            {
                return rs;
            }
            #endregion

            //数据对比的两种情况：
            //1、门岗结果集没有数据
            //2、有进出门岗的信息，但是和电子围栏异常信息时间间隔相差10（可配置）分钟以上

            DataTable tempDt = new DataTable();
            tempDt.Columns.Add("PlateNum", typeof(string));
            tempDt.Columns.Add("ExceptionInfo", typeof(string));
            tempDt.Columns.Add("SignalStartTime", typeof(string));
            tempDt.Columns.Add("SignalEndTime", typeof(string));

            #region 1、门岗结果集没有数据
            if (gateDt.Rows.Count == 0)
            {
                for (int i = 0; i < efDt.Rows.Count; i++)
                {
                    tempDt.Rows.Add(efDt.Rows[i]["PlateNum"], "都不匹配", efDt.Rows[i]["SignalStartTime"], efDt.Rows[i]["SignalEndTime"]);
                }
            }
            #endregion

            #region 2、有进出门岗的信息
            else
            {
                //分析电子围栏信息时  门岗信息要同时有满足开始时间间隔10分钟内的进入门岗系统信息  和  满足结束时间间隔10分钟内的驶出门岗系统信息，否则记录异常
                for (int i = 0; i < efDt.Rows.Count; i++)
                {
                    string plateNum = FontHelper.StringConvert(efDt.Rows[i]["PlateNum"].ToString(), "1");
                    //查询电子围栏记录开始时间 正负10分钟内进入门岗系统的记录
                    string formIn = string.Format(" {0}='{1}' AND {2}='{3}' AND {4} >= '{5}' AND  {6} <= '{7}' ",
                        "PlateNum", plateNum, "InOrOut", "入口",
                        "InOrOutTime", Convert.ToDateTime(efDt.Rows[i]["SignalStartTime"]).AddMinutes(-timeInterval).ToString("yyyy-MM-dd HH:mm:ss"),
                        "InOrOutTime", Convert.ToDateTime(efDt.Rows[i]["SignalStartTime"]).AddMinutes(timeInterval).ToString("yyyy-MM-dd HH:mm:ss"));
                    var InCount = gateDt.Select(formIn).Count();

                    //查询电子围栏记录结束时间 正负10分钟内出了门岗系统的记录
                    string formOut = string.Format(" {0}='{1}' AND {2}='{3}' AND {4} >= '{5}' AND  {6} <= '{7}' ",
                        "PlateNum", plateNum, "InOrOut", "出口",
                        "InOrOutTime", Convert.ToDateTime(efDt.Rows[i]["SignalEndTime"]).AddMinutes(-timeInterval).ToString("yyyy-MM-dd HH:mm:ss"),
                        "InOrOutTime", Convert.ToDateTime(efDt.Rows[i]["SignalEndTime"]).AddMinutes(timeInterval).ToString("yyyy-MM-dd HH:mm:ss"));
                    var OutCount = gateDt.Select(formOut).Count();

                    //进入驶出都有记录时候不算异常，其余条件算异常
                    //电子围栏异常开始时间匹配不到门岗信息
                    if (InCount < 1 && OutCount > 0)
                    {
                        tempDt.Rows.Add(efDt.Rows[i]["PlateNum"], "开始时间不匹配", efDt.Rows[i]["SignalStartTime"], efDt.Rows[i]["SignalEndTime"]);
                    }
                    //电子围栏异常结束时间匹配不到门岗信息
                    if (InCount > 0 && OutCount < 1)
                    {
                        tempDt.Rows.Add(efDt.Rows[i]["PlateNum"], "结束时间不匹配", efDt.Rows[i]["SignalStartTime"], efDt.Rows[i]["SignalEndTime"]);
                    }
                    //电子围栏异常  开始时间和结束时间都匹配不到门岗信息
                    if (InCount < 1 && OutCount < 1)
                    {
                        tempDt.Rows.Add(efDt.Rows[i]["PlateNum"], "都不匹配", efDt.Rows[i]["SignalStartTime"], efDt.Rows[i]["SignalEndTime"]);
                    }
                }
            }
            #endregion

            return ConvertToList<GateExceptionModel>.Convert(tempDt);
        }

        #endregion


        #region ACCON时长统计报表
        //默认模式
        public static List<ACCONStatisticDataModel> GetDefaultACCONStatisticData(ACCONStatisticSearchModel model, int strucID)
        {
            // 获取时间段  根据时间段查询对应的信号月表 月表的时间格式是yyyyMM
            string month = model.SartTime.ToString("yyyyMM");
            string whereSql = string.Empty;
            // 如果选择了车辆 则只需要查询当前车辆的信息
            if (model.VIN != null && model.VIN != "")
            {
                whereSql = string.Format(" WHERE uv.VIN = '{0}' ", model.VIN);
            }
            // 根据单位ID获取当前用户能查看到的所有车辆信息
            string tSql = string.Format(@"SELECT uv.VIN,s.StrucName,uv.VehicleName FROM Func_GetAllTheSubsetOfVehiclesByStrucID({0}) AS uv
                                                   INNER JOIN Structures AS s ON uv.StrucID = s.ID {1};", strucID, whereSql);
            DataTable dtInfo = MSSQLHelper.ExecuteDataTable(CommandType.Text, tSql);

            // 这里构造sql语句比较长 所以使用StringBuilder
            StringBuilder sql = new StringBuilder();
            if (dtInfo != null && dtInfo.Rows.Count > 0)
            {
                int count = dtInfo.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    string vin = dtInfo.Rows[i]["VIN"].ToString();
                    string strucName = dtInfo.Rows[i]["StrucName"].ToString();
                    string vehicleName = dtInfo.Rows[i]["VehicleName"].ToString();
                    string unionAll = " UNION ALL ";
                    if (i == count - 1)
                    {
                        unionAll = string.Empty;
                    }
                    // 根据条件查询所有的历史轨迹信息
                    sql.AppendFormat(@"  SELECT '{0}' AS StrucName,'{1}' AS VehicleName,[SignalDateTime],[ACCState]
                                                        FROM GNSS.dbo.[{2}] with(nolock) WHERE VIN ='{3}' AND SignalDateTime 
                                                        BETWEEN '{4}' AND '{5}'  {6}  ",
                                  strucName, vehicleName, month, vin, model.SartTime, model.EndTime, unionAll);
                }
            }
            List<ACCONStatisticHisgModel> dtSigs = ConvertToList<ACCONStatisticHisgModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql.ToString()));
            
            #region 统计ACCON时长
            List<ACCONStatisticDataModel> result = new List<ACCONStatisticDataModel>();

            try {
                if (dtSigs != null && dtSigs.Count > 0)
                {
                    //分组
                    var query = from data in dtSigs
                                group data by new { data.VehicleName, data.StrucName }
                                    into groupedResult
                                    select new
                                    {
                                        VehicleName = groupedResult.Key.VehicleName,
                                        StrucName = groupedResult.Key.StrucName,
                                        GroupList = groupedResult
                                    };
                    //求每个分组的ACCON统计时长
                    foreach (var q in query)
                    {
                        System.Collections.Generic.List<ACCONStatisticHisgModel> groupSigs = q.GroupList.OrderBy(a => a.SignalDateTime).ToList();
                        double ss = 0;//总时长（秒）
                        if (groupSigs != null && groupSigs.Count > 1)
                        {
                            DateTime sTime = groupSigs[0].SignalDateTime, eTime;//记录每个时间段的开始结束时间
                            for (int i = 0; i < groupSigs.Count; i++)
                            {
                                //判断开始时间，第一个点单独判断
                                if (i == 0)
                                {
                                    if (groupSigs[i].ACCState == true) { sTime = groupSigs[i].SignalDateTime; }
                                }
                                else if (groupSigs[i].ACCState == true && groupSigs[i - 1].ACCState == false)
                                {
                                    sTime = groupSigs[i].SignalDateTime;
                                }
                                //判断结束时间，最后一个点单独判断
                                if (i == groupSigs.Count - 1)
                                {
                                    if(groupSigs[i - 1].ACCState == true && groupSigs[i].ACCState == true)
                                    {
                                        eTime = groupSigs[i].SignalDateTime;
                                        ss += eTime.Subtract(sTime).TotalSeconds;
                                    }
                                }
                                else if (groupSigs[i].ACCState == true && groupSigs[i + 1].ACCState == false)
                                {
                                    eTime = groupSigs[i].SignalDateTime;
                                    ss += eTime.Subtract(sTime).TotalSeconds;
                                }
                            }
                        }
                        result.Add(new ACCONStatisticDataModel { StrucName = q.StrucName, VehicleName = q.VehicleName, StartDateTime = model.SartTime, EndDateTime = model.EndTime, TotalTime = ss });
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.DoOtherErrorLog(ex.ToString());
            }
            
            #endregion

            return result;
        }


        //自由模式
        public static List<ACCONStatisticDataModel> GetACCONStatisticData(ACCONStatisticSearchModel model, int uid)
        {
            // 获取时间段  根据时间段查询对应的信号月表 月表的时间格式是yyyyMM
            string month = model.SartTime.ToString("yyyyMM");
            string whereSql = string.Empty;
            // 如果选择了车辆 则只需要查询当前车辆的信息
            if (model.VIN != null && model.VIN != "")
            {
                whereSql = string.Format(" WHERE uv.VIN = '{0}' ", model.VIN);
            }
            // 根据单位ID获取当前用户能查看到的所有车辆信息
            string tSql = string.Format(@"SELECT uv.VIN,s.StrucName,uv.VehicleName FROM Func_GetVehiclesListByUserID_New({0}) AS uv
                                                   INNER JOIN Structures AS s ON uv.StrucID = s.ID {1};", uid, whereSql);
            DataTable dtInfo = MSSQLHelper.ExecuteDataTable(CommandType.Text, tSql);

            // 这里构造sql语句比较长 所以使用StringBuilder
            StringBuilder sql = new StringBuilder();
            if (dtInfo != null && dtInfo.Rows.Count > 0)
            {
                int count = dtInfo.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    string vin = dtInfo.Rows[i]["VIN"].ToString();
                    string strucName = dtInfo.Rows[i]["StrucName"].ToString();
                    string vehicleName = dtInfo.Rows[i]["VehicleName"].ToString();
                    string unionAll = " UNION ALL ";
                    if (i == count - 1)
                    {
                        unionAll = string.Empty;
                    }
                    // 根据条件查询所有的历史轨迹信息
                    sql.AppendFormat(@"  SELECT '{0}' AS StrucName,'{1}' AS VehicleName,[SignalDateTime],[ACCState]
                                                        FROM GNSS.dbo.[{2}] with(nolock) WHERE VIN ='{3}' AND SignalDateTime 
                                                        BETWEEN '{4}' AND '{5}'  {6}  ",
                                  strucName, vehicleName, month, vin, model.SartTime, model.EndTime, unionAll);
                }
            }
            List<ACCONStatisticHisgModel> dtSigs = ConvertToList<ACCONStatisticHisgModel>.Convert(MSSQLHelper.ExecuteDataTable(CommandType.Text, sql.ToString()));

            #region 统计ACCON时长
            List<ACCONStatisticDataModel> result = new List<ACCONStatisticDataModel>();

            try
            {
                if (dtSigs != null && dtSigs.Count > 0)
                {
                    //分组
                    var query = from data in dtSigs
                                group data by new { data.VehicleName, data.StrucName }
                                    into groupedResult
                                    select new
                                    {
                                        VehicleName = groupedResult.Key.VehicleName,
                                        StrucName = groupedResult.Key.StrucName,
                                        GroupList = groupedResult
                                    };
                    //求每个分组的ACCON统计时长
                    foreach (var q in query)
                    {
                        System.Collections.Generic.List<ACCONStatisticHisgModel> groupSigs = q.GroupList.OrderBy(a => a.SignalDateTime).ToList();
                        double ss = 0;//总时长（秒）
                        if (groupSigs != null && groupSigs.Count > 1)
                        {
                            DateTime sTime = groupSigs[0].SignalDateTime, eTime;//记录每个时间段的开始结束时间
                            for (int i = 0; i < groupSigs.Count; i++)
                            {
                                //判断开始时间，第一个点单独判断
                                if (i == 0)
                                {
                                    if (groupSigs[i].ACCState == true) { sTime = groupSigs[i].SignalDateTime; }
                                }
                                else if (groupSigs[i].ACCState == true && groupSigs[i - 1].ACCState == false)
                                {
                                    sTime = groupSigs[i].SignalDateTime;
                                }
                                //判断结束时间，最后一个点单独判断
                                if (i == groupSigs.Count - 1)
                                {
                                    if (groupSigs[i - 1].ACCState == true && groupSigs[i].ACCState == true)
                                    {
                                        eTime = groupSigs[i].SignalDateTime;
                                        ss += eTime.Subtract(sTime).TotalSeconds;
                                    }
                                }
                                else if (groupSigs[i].ACCState == true && groupSigs[i + 1].ACCState == false)
                                {
                                    eTime = groupSigs[i].SignalDateTime;
                                    ss += eTime.Subtract(sTime).TotalSeconds;
                                }
                            }
                        }
                        result.Add(new ACCONStatisticDataModel { StrucName = q.StrucName, VehicleName = q.VehicleName, StartDateTime = model.SartTime, EndDateTime = model.EndTime, TotalTime = ss });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.DoOtherErrorLog(ex.ToString());
            }

            #endregion

            return result;
        }

        #endregion
    }
}