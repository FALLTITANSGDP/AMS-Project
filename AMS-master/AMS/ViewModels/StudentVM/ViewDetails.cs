﻿using AMS.Models;

namespace AMS.ViewModels.StudentVM
{
    public class ViewDetails
    {
        public Course_Section_Faculty Course_Section_Faculty { get; set; }
        public List<Students_Attendance> AttendanceList { get; set; }
    }
}
