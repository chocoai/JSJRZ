﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MXKJ.DBMiddleWareLib;

namespace MXKJ.Entity
{
    [TableAttrib("edu_moral_record", "ID")]
    public struct Moral_Record
    {
        [ColumnAttrib("ID")]
        public int? ID { get; set; }
        [ColumnAttrib("ClassID")]
        public int? ClassID { get; set; }
        [ColumnAttrib("Date")]
        public DateTime? Date { get; set; }
        [ColumnAttrib("StudentsID")]
        public String StudentsID { get; set; }
        [ColumnAttrib("StudentsName")]
        public String StudentsName { get; set; }
        [ColumnAttrib("TypeName")]
        public string TypeName { get; set; }
        [ColumnAttrib("Point")]
        public int? Point { get; set; }
        [ColumnAttrib("Memo")]
        public String Memo { get; set; }
    }

}
