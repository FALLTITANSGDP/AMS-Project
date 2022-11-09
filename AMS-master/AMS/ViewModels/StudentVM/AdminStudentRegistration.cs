using AMS.Models;

namespace AMS.ViewModels.StudentVM
{
    public class AdminStudentRegistration
    {
        public List<Student> Students { get; set; }
        public List<Course_Section_Faculty> Course_Section_Faculty { get; set; }
    }
}
