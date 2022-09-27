using AMS.Models;
using AMS.Services;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using AMS.ViewModels;
using AMS.ViewModels.Faculty;
using AMS.ViewModels.Student;
using static QRCoder.PayloadGenerator;
using NuGet.Common;

namespace AMS.Controllers
{
    [Authorize]
    public class AMSController : BaseController
    {
        private readonly IDbOperations dbOperations;
        private readonly IHttpContextAccessor httpContextAccessor;
        public AMSController(IDbOperations dbOperations, IHttpContextAccessor httpContextAccessor)
        {
            this.dbOperations = dbOperations;
            this.httpContextAccessor = httpContextAccessor;
        }

        #region Section
        public IActionResult Section()
        {
            return View();
        }

        public async Task<Section> SaveSection(Section section)
        {
            try
            {
                var result = await dbOperations.SaveData(section, "Section");
                if (result == null)
                {

                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList<Section>> GetSection()
        {
            try
            {
                var result = await dbOperations.GetAllData<Section>("Section");
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion Section

        #region Course
        public IActionResult Course()
        {
            return View();
        }

        public async Task<Course?> SaveCourse(Course course)
        {
            try
            {
                var result = await dbOperations.SaveData(course, "Course");
                if (result == null)
                {

                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList<Course>> GetCourse()
        {
            try
            {
                var result = await dbOperations.GetAllData<Course>("Course");
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        #endregion Course

        #region Faculty
        public IActionResult Faculty()
        {
            return View();
        }

        public async Task<Faculty> SaveFaculty(Faculty faculty)
        {
            try
            {

                var result = await dbOperations.SaveData(faculty, "Faculty");
                if (result == null)
                {

                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList<Faculty>> GetFaculty()
        {
            try
            {
                var result = await dbOperations.GetAllData<Faculty>("Faculty");
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion Faculty

        #region Student
        public IActionResult Student()
        {
            return View();
        }

        public async Task<IActionResult> SaveStudent(Student student)
        {
            try
            {
                var result = await dbOperations.SaveData(student, "Student");
                if (result == null)
                {

                }
                return View("Student");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> GetStudent()
        {
            try
            {
                var result = await dbOperations.GetAllData<Student>("Student");
                return View();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion Student

        #region FacultyRegistration
        public async Task<IActionResult> FacultyRegistration()
        {
            var courseList = await dbOperations.GetAllData<Course>("Course");
            var sectionList = await dbOperations.GetAllData<Section>("Section");
            var registrationList = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
            var facultyEmail = HttpContext.Session.GetString("UserEmail");
            FacultyRegistrationViewModel data = new()
            {
                Courses = courseList,
                Sections = sectionList,
                RegistrationList = registrationList.Where(x => x.Faculty.Email.Equals(facultyEmail, StringComparison.OrdinalIgnoreCase)).ToList()
            };
            return View(data);
        }

        public async Task<IActionResult> ViewRegCourseDetails(string data)
        {
            try
            {
                ViewDetails viewDetails = new();
                var facultyRegList = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
                var selectedCourseDetails = facultyRegList.FirstOrDefault(x => x.Id == data);
                var attendanceLise = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
                if (selectedCourseDetails != null)
                {
                    viewDetails = new ViewDetails
                    {
                        Course_Section_Faculty = selectedCourseDetails,
                        AttendanceList = attendanceLise.Where(x => x.Student_Course_Registration.Course_Section_Faculty.Id == data).ToList()
                    };
                }

                return View(viewDetails);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> RegisterFacultyWithCourse(CourseViewModel courseViewModel)
        {
            try
            {
                if (courseViewModel == null || courseViewModel.SName == null || courseViewModel.CName == null)
                {
                    //With Fail Message
                    return View("FacultyRegistration");
                }
                Course_Section_Faculty data = new() { };
                var courseList = await dbOperations.GetAllData<Course>("Course");
                var sectionList = await dbOperations.GetAllData<Section>("Section");
                var facultyList = await dbOperations.GetAllData<Faculty>("Faculty");
                if (courseList != null && courseList.Count > 0 && sectionList != null && sectionList.Count > 0 && facultyList != null && facultyList.Count > 0)
                {
                    var facultyInfo = facultyList.FirstOrDefault(x => x.Email == HttpContext.Session.GetString("UserEmail"));
                    var selectedCourse = courseList.FirstOrDefault(x => x.Name == courseViewModel.CName);
                    var selectedSection = sectionList.FirstOrDefault(x => x.Name == courseViewModel.SName);
                    if (selectedCourse != null && selectedSection != null && facultyInfo != null)
                    {
                        data.Course = selectedCourse;
                        data.Section = selectedSection;
                        data.Faculty = facultyInfo;
                    }
                }
                if (data.Course == null || data.Section == null)
                {
                    //With Fail Message
                    return View("FacultyRegistration");
                }
                var result = await dbOperations.SaveData(data, "Course_Section_Faculty");
                if (result == null)
                {
                    //With Success Message
                    return RedirectToAction("FacultyRegistration");
                }
                return RedirectToAction("FacultyRegistration");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion FacultyRegistration

        #region StudentCourseRegistration
        public async Task<IActionResult> StudentRegistration()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var scheduledCourses = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
            var courseRegistered = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
            courseRegistered = courseRegistered.Where(x => x.Student.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).ToList();
            IList<StudentRegistrationViewModel> studentRegistrationViewModel = new List<StudentRegistrationViewModel>();
            foreach (var sCourse in scheduledCourses)
            {
                var data = courseRegistered.FirstOrDefault(x => x.Course_Section_Faculty.Course.Id.Equals(sCourse.Course.Id, StringComparison.OrdinalIgnoreCase) && x.Course_Section_Faculty.Faculty.Id.Equals(sCourse.Faculty.Id, StringComparison.OrdinalIgnoreCase) && x.Course_Section_Faculty.Section.Id.Equals(sCourse.Section.Id, StringComparison.OrdinalIgnoreCase));
                if (courseRegistered.Count > 0 && data != null)
                {
                    var courseRegisteredId = data.Id;
                    if (courseRegisteredId == null)
                    {
                        return View(studentRegistrationViewModel);
                    }
                    StudentRegistrationViewModel studentRegistration = new()
                    {
                        Course_Section_Faculty = sCourse,
                        CourseRegisteredId = courseRegisteredId,
                        IsRegistered = true
                    };
                    studentRegistrationViewModel.Add(studentRegistration);
                }
                else
                {
                    StudentRegistrationViewModel studentRegistration = new()
                    {
                        Course_Section_Faculty = sCourse,
                        IsRegistered = false
                    };
                    studentRegistrationViewModel.Add(studentRegistration);
                }
            }
            return View(studentRegistrationViewModel);
        }

        public async Task<IActionResult> RegisterStudentWithCourse(string data)
        {
            try
            {
                if (data == null)
                {
                    //Fail Message
                    return RedirectToAction("StudentRegistration", "AMS");
                }
                Student_Course_Registration student_Course_Registration = new Student_Course_Registration();
                var course_Section_Faculty = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
                var studentList = await dbOperations.GetAllData<Student>("Student");
                var selectedCourse_Section_Faculty = course_Section_Faculty.FirstOrDefault(x => x.Id == data);
                if (selectedCourse_Section_Faculty != null && studentList.Count > 0)
                {
                    var student = studentList.FirstOrDefault(x => x.Email.Equals(HttpContext.Session.GetString("UserEmail"), StringComparison.OrdinalIgnoreCase));
                    if (student != null)
                    {
                        student_Course_Registration.Course_Section_Faculty = selectedCourse_Section_Faculty;
                        student_Course_Registration.Student = student;
                    }
                }
                if (student_Course_Registration.Student == null || student_Course_Registration.Course_Section_Faculty == null)
                {
                    //Message with Error
                    return RedirectToAction("StudentRegistration");
                }
                var result = await dbOperations.SaveData(student_Course_Registration, "Student_Course_Registration");
                if (result == null)
                {
                    //Message with Error
                    return RedirectToAction("StudentRegistration");
                }
                return RedirectToAction("StudentRegistration");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> CancelRegisterOfCourse(string data)
        {
            try
            {
                if (data == null)
                {
                    //Message with Error
                    return RedirectToAction("StudentRegistration");
                }
                var result = await dbOperations.DeleteData(data, "Student_Course_Registration");
                if (!result)
                {
                    //Message with Error
                    return RedirectToAction("StudentRegistration");
                }
                return RedirectToAction("StudentRegistration");
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion StudentCourseRegistration

        #region Attendance

        public async Task<IActionResult> ViewMyAttendance()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var attendanceList = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
            attendanceList = attendanceList.Where(x => x.Student_Course_Registration.Student.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).ToList();
            return View(attendanceList);
        }

        [AllowAnonymous]
        public IActionResult MarkAttendance(string uid)
        {
            ViewBag.CId = uid;
            return View();
        }
        [AllowAnonymous]
        public async Task<IActionResult> SubmitAttendance(SubmitAttendanceViewModel data)
        {
            try
            {
                var pinList = await dbOperations.GetAllData<UPIN>("UPIN");

                var studentList = await dbOperations.GetAllData<Student>("Student");
                if (!studentList.Any(x => x.Email.Equals(data.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    ViewData["Invalid"] = "Studnet not exit";
                    return View();
                }
                var userPINDetails = pinList.FirstOrDefault(x => x.Email.Equals(data.Email, StringComparison.OrdinalIgnoreCase));
                if (userPINDetails.PIN != data.PIN)
                {
                    ViewData["Invalid"] = "Invalid PIN";
                    return View();
                }
                var student_Course_Registration = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
                var studentCourseRegistered = student_Course_Registration.FirstOrDefault(x => x.Student.Email.Equals(data.Email, StringComparison.OrdinalIgnoreCase) && x.Course_Section_Faculty.Id.Equals(data.CId, StringComparison.OrdinalIgnoreCase));
                if (studentCourseRegistered != null)
                {
                    var result = await dbOperations.SaveData<Students_Attendance>(new Students_Attendance
                    {
                        IsAttended = true,
                        Student_Course_Registration = studentCourseRegistered
                    }, "Students_Attendance");
                    if (result != null)
                    {
                        ViewData["Valid"] = "Submitted";
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            ViewData["InValid"] = "Some thing went wrong ..!";
            return View();
        }

        public async Task<IActionResult> UpdateAttendance(string cId, string email, string data, bool isApproved)
        {
            var studentAttendanceList = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
            var currentAttendance = studentAttendanceList.FirstOrDefault(x => x.Student_Course_Registration.Student.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && x.Id == data);
            if (currentAttendance != null)
            {
                currentAttendance.IsApproved = isApproved;
                var updatedData = await dbOperations.UpdateData<Students_Attendance>(data, currentAttendance, "Students_Attendance");
                return RedirectToAction("ViewRegCourseDetails", "AMS", new { data = cId });
            }
            return RedirectToAction("ViewRegCourseDetails", "AMS", new { data = cId });
        }
        #endregion Attendance

        #region QRCode
        [HttpGet]
        public IActionResult CreateQRCode(string data)
        {
            var url = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "/AMS/MarkAttendance?uid=" + data;
            string WebUri = new Uri(url).ToString();
            string UriPayload = WebUri.ToString();
            QRCodeGenerator QrGenerator = new();
            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(UriPayload, QRCodeGenerator.ECCLevel.Q);
            QRCode QrCode = new QRCode(QrCodeInfo);
            Bitmap QrBitmap = QrCode.GetGraphic(60);
            byte[] BitmapArray = QrBitmap.BitmapToByteArray();
            string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
            ViewBag.QrCodeUri = QrUri;
            return View();
        }
        #endregion QRCode



        #region MyProfile
        public IActionResult MyProfile()
        {

            ProfileViewModel profile = new ProfileViewModel
            {
                Email = HttpContext.Session.GetString("UserEmail"),
                Name = HttpContext.Session.GetString("UserName"),
                UserType = HttpContext.Session.GetString("_UserType"),
                Address = "N/A",
                PhoneNumber = "N/A"
            };
            return View(profile);
        }

        public async Task<IActionResult> CreatedPIN(int pin)
        {
            UPIN pinDetails = new()
            {
                Email = HttpContext.Session.GetString("UserEmail"),
                PIN = pin
            };
            var result = await dbOperations.SaveData<UPIN>(pinDetails, "UPIN");
            return View();
        }

        public async Task<int> GetPIN()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var pinDetails = await dbOperations.GetAllData<UPIN>("UPIN");
            var pin = pinDetails.FirstOrDefault(x => x.Email.Equals(email))?.PIN;
            return pin ?? 0000;
        }

        [HttpPost]
        public async Task<IActionResult> ResetPIN(int PIN)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var pinDetailsList = await dbOperations.GetAllData<UPIN>("UPIN");
            var currentPinDetails = pinDetailsList.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (currentPinDetails != null)
            {
                currentPinDetails.PIN = PIN;
                var result = await dbOperations.UpdateData<UPIN>(currentPinDetails.Id, currentPinDetails, "UPIN");
                if (result.PIN == PIN)
                {
                    HttpContext.Session.SetString("IsPINSet", "TRUE");
                    return RedirectToAction("MyProfile", "AMS");
                }
            }
            else
            {
                currentPinDetails = new UPIN
                {
                    Email = email,
                    PIN = PIN,
                };
                var result = await dbOperations.SaveData<UPIN>(currentPinDetails, "UPIN");
                if (result.PIN == PIN)
                {
                    HttpContext.Session.SetString("IsPINSet", "TRUE");
                    return RedirectToAction("MyProfile", "AMS");
                }
            }
            HttpContext.Session.SetString("IsPINSet", "FALSE");
            return RedirectToAction("MyProfile", "AMS");
        }
        #endregion MyProfile

    }
}
