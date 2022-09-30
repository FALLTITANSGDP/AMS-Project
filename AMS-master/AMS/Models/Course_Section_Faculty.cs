namespace AMS.Models
{
    public class Course_Section_Faculty : BaseModel
    {
        public Course Course { get; set; }
        public Section Section { get; set; }
        public Faculty Faculty { get; set; }
        public DateTime LastClassOn { get; set; }
        public int TotalCount { get; set; }
    }
}
