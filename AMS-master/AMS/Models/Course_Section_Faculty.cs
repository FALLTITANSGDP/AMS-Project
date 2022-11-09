namespace AMS.Models
{
    public class Course_Section_Faculty : BaseModel
    {

        public string DisplayName
        {
            get { return Course.Name + "-" + Section.Name + "-" + Faculty.FirstName + " " + Faculty.LastName; }
        }
        public Course Course { get; set; }
        public Section Section { get; set; }
        public Faculty Faculty { get; set; }
        public DateTime LastClassOn { get; set; }
        public bool IsApproved { get; set; }
        public int TotalCount { get; set; }
        public Timer addTimer
        { get; set; }
    }
}
