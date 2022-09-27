using AMS.Models;

namespace AMS.ViewModels.Faculty
{
    public class FacultyRegistrationViewModel
    {
        public List<Course> Courses { get; set; }
        public List<Section> Sections { get; set; }
        public List<Course_Section_Faculty> RegistrationList { get; set; }
    }
}
