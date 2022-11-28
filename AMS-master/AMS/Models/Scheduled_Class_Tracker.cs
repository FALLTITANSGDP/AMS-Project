namespace AMS.Models
{
    public class Scheduled_Class_Tracker : BaseModel
    {
        public Course_Section_Faculty course_Section_Faculty { get; set; }
        public DateTime ClassOn { get; set; }
    }
}
