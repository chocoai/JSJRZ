﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MXKJ.JSJRZ.WebUI.Models.SafeManager
{
    public class HiddenDangerInfoViewModel
    {
        public List<HiddenDangerInfoItemViewModel> ItemList { get; set; } = new List<HiddenDangerInfoItemViewModel>();

    }

    public class HiddenDangerInfoItemViewModel
    {
        public int ID { get; set; }
        //[Required]
        //[Display(Name = "组织者")]
        public String Organizer { get; set; }
        //[Required]
        //[Display(Name = "检查时间")]
        //[RegularExpression(Regular.Regular_Date, ErrorMessage = "请输入正确的时间")]
        public DateTime CheckTime { get; set; }
        public String Participant { get; set; }
        public String RectificationMeasures { get; set; }
        public String Address { get; set; }

    }
}