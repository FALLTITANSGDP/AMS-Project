using AMS.Models;

namespace AMS.ViewModels.FacultyVM
{
    public class FacultyRegistrationViewModel
    {
        public List<Faculty> Faculties { get; set; }
        public List<Course> Courses { get; set; }
        public List<Section> Sections { get; set; }
        public List<Course_Section_Faculty> RegistrationList { get; set; }
    }
}
