namespace AMS.ViewModels.FacultyVM
{
    public class StudentAttendanceViewModel
    {
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string CourseDetails { get; set; }
        public DateTime ClassOn { get; set; }
        public bool IsAttendand { get; set; }
    }
}
