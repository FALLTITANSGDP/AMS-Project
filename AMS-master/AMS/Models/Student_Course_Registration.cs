namespace AMS.Models
{
    public class Student_Course_Registration: BaseModel
    {
        public Student Student { get; set; }
        public Course_Section_Faculty Course_Section_Faculty { get; set; }
    }
}
