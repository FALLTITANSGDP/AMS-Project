using AMS.Models;

namespace AMS.ViewModels.StudentVM
{
    public class StudentRegistrationViewModel
    {
        public List<Student> Students { get; set; }
        public Course_Section_Faculty Course_Section_Faculty { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsApproved { get; set; }
        public string CourseRegisteredId { get; set; }
    }
}
