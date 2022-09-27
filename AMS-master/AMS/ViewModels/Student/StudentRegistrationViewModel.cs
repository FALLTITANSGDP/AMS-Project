using AMS.Models;

namespace AMS.ViewModels.Student
{
    public class StudentRegistrationViewModel
    {
        public Course_Section_Faculty Course_Section_Faculty { get; set; }
        public bool IsRegistered { get; set; }
        public string CourseRegisteredId { get; set; }
    }
}
