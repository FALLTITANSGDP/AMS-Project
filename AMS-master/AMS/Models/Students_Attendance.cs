namespace AMS.Models
{
    public class Students_Attendance : BaseModel
    {
        public Student_Course_Registration Student_Course_Registration { get; set; }
        public DateTime AttendedOn { get; set; }
        public bool IsAttended { get; set; }
        public DateTime ApprovedOn { get; set; }
        public bool IsApproved { get; set; }
        public string Comments { get; set; }
    }
}
