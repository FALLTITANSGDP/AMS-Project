using AMS.Models;

namespace AMS.ViewModels.StudentVM
{
    public class AdminStudentRegistrationViewModel
    {
        public List<Student> Students { get; set; }
        public List<Course_Section_Faculty> Course_Section_Faculty { get; set; }
        public List<Student_Course_Registration> RegistrationList { get; set; }
    }
}
