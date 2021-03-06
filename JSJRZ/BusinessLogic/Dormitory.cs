﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MXKJ.Entity;
using System.Data.OleDb;
using System.Data;
using Microsoft.Office.Interop.Excel;

namespace MXKJ.BusinessLogic
{
    /// <summary>
    /// 宿舍管理
    /// </summary>
    public class Dormitory : BusinessLogicBase
    {
        #region 公寓管理
        public Dormitory_ItemsEF[] GetAllDormitory()
        {
            return m_BasicDBClass.SelectAllRecordsEx<Dormitory_ItemsEF>();
        }

        public bool AddDormitory(Dormitory_ItemsEF Dormitory)
        {
            return m_BasicDBClass.InsertRecord(Dormitory) == -1 ? false : true;
        }

        public Dormitory_ItemsEF GetDormitoryInfo(int ID)
        {
            Dormitory_ItemsEF vDormitory = m_BasicDBClass.SelectRecordByPrimaryKeyEx<Dormitory_ItemsEF>(ID);
            return vDormitory;
        }

        public bool UpdateDormitoryInfo(Dormitory_ItemsEF DormitoryInfo)
        {
            return m_BasicDBClass.UpdateRecord(DormitoryInfo);
        }

        public bool DeleteDormitory(string IDStr)
        {
            bool vResult = false;
            if (IDStr.Length > 0)
                IDStr = IDStr.Remove(IDStr.Length - 1);
            IDStr = IDStr.Replace('|', ',');
            m_BasicDBClass.TransactionBegin();
            vResult = m_BasicDBClass.DeleteRecordCustom<Dormitory_ItemsEF>(string.Format("ID in ({0})", IDStr));
            if (vResult)
            {
                string vUpdateSql = string.Format("update  edu_students set HouseID=null where HouseID in ( Select ID from edu_dormitory_house where edu_dormitory_house.DormitoryID in ({0}))", IDStr);
                m_BasicDBClass.UpdateRecord(vUpdateSql);
                vResult = m_BasicDBClass.DeleteRecordCustom<Dormitory_HouseEF>( string.Format("DormitoryID in ({0})",IDStr));
            }
            if (vResult)
                m_BasicDBClass.TransactionCommit();
            else
                m_BasicDBClass.TransactionRollback();
            return vResult;
        }
        #endregion

        #region 房间管理
        public Dormitory_HouseViewEF[] GetAllHouseInfo()
        {
            return m_BasicDBClass.SelectAllRecordsEx<Dormitory_HouseViewEF>("ID desc", "*");
        }
        public Dormitory_HouseViewEF GetHouseInfoByID(int ID)
        {
            return m_BasicDBClass.SelectRecordByPrimaryKeyEx<Dormitory_HouseViewEF>(ID);
        }

        public Dormitory_HouseViewEF[] GetHouseInfoByDormitory(int DormitoryID)
        {
            Dormitory_HouseViewEF[] vResult = new Dormitory_HouseViewEF[0];
            Dormitory_HouseViewEF vHouseEF = new Dormitory_HouseViewEF();
            vHouseEF.DormitoryID = DormitoryID;
            vResult = m_BasicDBClass.SelectRecordsEx(vHouseEF);
            return vResult;
        }

        public Dormitory_HouseViewEF[] QueryHouseInfoByDormitory(int DormitoryID, int Floor)
        {
            Dormitory_HouseViewEF[] vResult = new Dormitory_HouseViewEF[0];
            Dormitory_HouseViewEF vHouseEF = new Dormitory_HouseViewEF();
            if (DormitoryID == 0)
                vResult = m_BasicDBClass.SelectAllRecordsEx<Dormitory_HouseViewEF>();
            else if (Floor == 0)
            {
                vHouseEF.DormitoryID = DormitoryID;
                vResult = m_BasicDBClass.SelectRecordsEx(vHouseEF);
            }
            else
            {
                vHouseEF.DormitoryID = DormitoryID;
                vHouseEF.Floor = Floor;
                vResult = m_BasicDBClass.SelectRecordsEx(vHouseEF);
            }
            return vResult;
        }

        public bool UpdateHouseInfo(Dormitory_HouseEF House)
        {
            return m_BasicDBClass.UpdateRecord(House);
        }

        public bool AddHouse(Dormitory_HouseEF House)
        {
            return m_BasicDBClass.InsertRecord(House) > 0 ? true : false;
        }

        public bool DeleteHouse(string IDStr)
        {
            if (IDStr.Length>0 && IDStr[IDStr.Length-1] == '|')
                IDStr = IDStr.Remove(IDStr.Length - 1);
            IDStr = IDStr.Replace('|', ',');
            m_BasicDBClass.TransactionBegin();
            bool vResult = m_BasicDBClass.DeleteRecordCustom<Dormitory_HouseEF>(string.Format("ID in ({0})", IDStr));
            if (vResult)
            {
                string vUpdateSql = string.Format("Update edu_students set HouseID = NULL where HouseID in ({0})", IDStr);
                m_BasicDBClass.UpdateRecord(vUpdateSql);
                vResult = true;
            }
            if (vResult)
                m_BasicDBClass.TransactionCommit();
            else
                m_BasicDBClass.TransactionRollback();
            return vResult;
        }

        public bool CreateHouseByDormitory(int DormitoryID, string Unit, int Storey, int LyaerHouseNumber, int BedNumber, string Area)
        {
            bool vResult = false;
            Dormitory_ItemsEF vDormitoryInfo = m_BasicDBClass.SelectRecordByPrimaryKeyEx<Dormitory_ItemsEF>(DormitoryID);
            for (int i = 1; i <= Storey; i++)
            {
                for (int j = 1; j <= LyaerHouseNumber; j++)
                {
                    Dormitory_HouseEF vHouseData = new Dormitory_HouseEF()
                    {
                        DormitoryID = DormitoryID,
                        Area = Area,
                        BedNumber = BedNumber,
                        ResidueBed = BedNumber,
                        Floor = i,
                        IsUse = true,
                        Number = string.Format("{0}{1:D2}", i, j)
                    };
                    if (m_BasicDBClass.InsertRecord(vHouseData) > 0)
                        vResult = true;
                    else
                    {
                        vResult = false;
                        break;
                    }
                }
                if (!vResult)
                    break;
            }
            return vResult;
        }
        #endregion

        #region 房间分配

        /// <summary>
        /// 读取宿舍分配数据
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public bool ReadHouseAllotData( string FilePath)
        {
            Application vExcel = new Application();
            Workbook vWorkbook = vExcel.Application.Workbooks.Open(FilePath);
            Worksheet vWorksheet = (Worksheet)vWorkbook.Worksheets.get_Item(1);
            int vCount1 = vWorksheet.UsedRange.Rows.Count;
            int vCount2= vWorksheet.UsedRange.Columns.Count;
            int i = 3;
            bool vStop = false;
            while ( !vStop)
            {
                Microsoft.Office.Interop.Excel.Range rng = vWorksheet.Cells["A1"]["A1"];//公寓楼
                string aa = rng.Text;
                //if (vBuildName != null && vBuildName != "" && vBuildName != string.Empty)
                //{
                //vWorksheet.Cells.get_Range();
                Microsoft.Office.Interop.Excel.Range rng1 = vWorksheet.Cells["B1"]["B1"];//楼层
                    object[,] arry1 = (object[,])rng1.Value2;
                    Microsoft.Office.Interop.Excel.Range rng2 = vWorksheet.Cells["C1"]["C1"];//房间编号
                    Microsoft.Office.Interop.Excel.Range rng3 = vWorksheet.Cells["D1"]["D1"];//学号
                    Microsoft.Office.Interop.Excel.Range rng4 = vWorksheet.Cells["E1"]["E1"];//姓名
                //}
                //else
                //    vStop = true;
            }
            int vCount = vWorksheet.Rows.Count;
            return true;
        }
        public bool HouseAllot(int HouseID, int BedNumber, string OldStudentsID, string StudentsID, string StudentsName)
        {
            bool vResult = false;
            string[] vUsersArray = StudentsID.Split(',');
            if (vUsersArray.Length <= BedNumber)
            {
                m_BasicDBClass.TransactionBegin();
                if (OldStudentsID != null && OldStudentsID != "")
                    vResult = m_BasicDBClass.UpdateRecord(string.Format("Update edu_students Set HouseID=null Where ID in ({0})", OldStudentsID));
                else
                    vResult = true;
                if ( vResult )
                    vResult = m_BasicDBClass.UpdateRecord(string.Format("Update edu_students Set HouseID={0} Where ID in ({1})", HouseID, StudentsID));
                if (vResult)
                {
                    Dormitory_HouseEF vHouseEF = new Dormitory_HouseEF();
                    vHouseEF.ID = HouseID;
                    vHouseEF.StudentID = StudentsID;
                    vHouseEF.StudentName = StudentsName;
                    vHouseEF.ResidueBed = BedNumber - vUsersArray.Length;
                    vResult = m_BasicDBClass.UpdateRecord(vHouseEF);
                }
                if (vResult)
                    m_BasicDBClass.TransactionCommit();
                else
                    m_BasicDBClass.TransactionRollback();
            }
            return vResult;
        }
        #endregion

        #region 管理人员
        public bool DeleteAdmin( string IDStr)
        {
            if (IDStr.Length > 0)
                IDStr = IDStr.Remove(IDStr.Length - 1);
            IDStr = IDStr.Replace('|', ',');
            return m_BasicDBClass.DeleteRecordCustom<Dormitory_AdminEF>(string.Format("ID in ({0})", IDStr));
        }

        public Dormitory_AdminViewEF[] GetAllAdminInfo()
        {
            return m_BasicDBClass.SelectAllRecordsEx<Dormitory_AdminViewEF>();
        }

        public Dormitory_AdminViewEF[] QueryAdminInfoByDormitory(int DormitoryID, int Floor)
        {
            Dormitory_AdminViewEF [] vResult = new Dormitory_AdminViewEF[0];
            Dormitory_AdminViewEF vAdminEF = new Dormitory_AdminViewEF();
            if (DormitoryID == 0)
                vResult = m_BasicDBClass.SelectAllRecordsEx<Dormitory_AdminViewEF>();
            else if (Floor == 0)
            {
                vAdminEF.Dormitory = DormitoryID;
                vResult = m_BasicDBClass.SelectRecordsEx(vAdminEF);
            }
            else
            {
                vAdminEF.Dormitory = DormitoryID;
                vAdminEF.Floor = Floor;
                vResult = m_BasicDBClass.SelectRecordsEx(vAdminEF);
            }
            return vResult;
        }

        public Dormitory_AdminViewEF GetAdminInfoByID( int AdminID )
        {
            return m_BasicDBClass.SelectRecordByPrimaryKeyEx<Dormitory_AdminViewEF>(AdminID);
        }

        public bool AddAdmin( string Name, string WorkType, string WorkTime,
            int Dormitory, int? Floor, string Duty, string Tel, string Memo)
        {
            Dormitory_AdminEF vAdminEF = new Dormitory_AdminEF()
            {
                Dormitory = Dormitory,
                Duty = Duty,
                Floor = Floor,
                Memo = Memo,
                Name = Name,
                Tel = Tel,
                WorkTime = WorkTime,
                WorkType = WorkType
            };
            return m_BasicDBClass.InsertRecord(vAdminEF)>=0?true:false ;
        }

        public bool UpdateAdminInfo( int AdminID,string Name,string WorkType,string WorkTime,
            int Dormitory,int? Floor,string Duty,string Tel,string Memo)
        {
            Dormitory_AdminEF vAdminEF = new Dormitory_AdminEF()
            {
                ID = AdminID,
                Dormitory = Dormitory,
                Duty = Duty,
                Floor = Floor,
                Memo = Memo,
                Name = Name,
                Tel = Tel,
                WorkTime = WorkTime,
                WorkType = WorkType
            };
            return m_BasicDBClass.UpdateRecord(vAdminEF);
        }
        #endregion


    }
}
