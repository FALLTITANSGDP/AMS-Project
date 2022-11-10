using AMS.Models;
using AMS.Services;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using AMS.ViewModels;
using AMS.ViewModels.StudentVM;
using static QRCoder.PayloadGenerator;
using NuGet.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using MessagePack.Resolvers;
using System.Numerics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Composition;
using System.Text;
using AMS.ViewModels.FacultyVM;
using AMS.ViewModels.StudentVM;

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

        public async Task<IActionResult> Section()
        {
            var section = await dbOperations.GetAllData<Section>("Section");
            return View(section);
        }

        public async Task<IActionResult> SaveSection(Section section)
        {
            try
            {
                var result = await dbOperations.SaveData(section, "Section");
                if (result == null)
                {
                    return RedirectToAction("Section", "AMS");
                }
                return RedirectToAction("Section", "AMS");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Section

        #region Course

        public async Task<IActionResult> Course()
        {
            var course = await dbOperations.GetAllData<Course>("Course");
            return View(course);
        }

        public async Task<IActionResult> SaveCourse(Course course)
        {
            try
            {
                var result = await dbOperations.SaveData(course, "Course");
                if (result == null)
                {
                    return RedirectToAction("Course", "AMS");
                }
                return RedirectToAction("Course", "AMS");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Course

        #region FacultyRegistration

        public async Task<IActionResult> FacultyDashboard()
        {
            FacultyDashboardViewModel facultyDashboardViewModel = new();
            ProfileViewModel profile = new()
            {
                UserId = HttpContext.Session.GetString("UserId"),
                Email = HttpContext.Session.GetString("UserEmail"),
                Name = HttpContext.Session.GetString("UserName"),
                UserType = HttpContext.Session.GetString("_UserType"),
                Address = "N/A",
                PhoneNumber = "N/A"
            };
            facultyDashboardViewModel.Profile = profile;
            var registrationList = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
            var facultyEmail = HttpContext.Session.GetString("UserEmail");
            var registrationCourse = registrationList.Where(x => x.Faculty.Email.Equals(facultyEmail, StringComparison.OrdinalIgnoreCase)).ToList();
            facultyDashboardViewModel.TiltesData = new List<TilesViewModel>();
            foreach (var item in registrationCourse)
            {
                TilesViewModel tilesViewModel = new TilesViewModel
                {
                    Id = item.Id,
                    DisplayContent = item.Course.Name,
                    Action = "ViewRegCourseDetails",
                    Controller = "AMS",
                    Description = "Section :" + item.Section.Name + "Timings :" + item.Section.TimeSlot
                };
                facultyDashboardViewModel.TiltesData.Add(tilesViewModel);
            }


            return View(facultyDashboardViewModel);
        }

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
                ViewBag.IsPeopleEnable = data;
                ViewDetails viewDetails = new();
                var facultyRegList = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
                var selectedCourseDetails = facultyRegList.FirstOrDefault(x => x.Id == data);
                var attendanceLise = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
                if (selectedCourseDetails != null)
                {
                    viewDetails = new ViewDetails
                    {
                        Course_Section_Faculty = selectedCourseDetails,
                        AttendanceList = attendanceLise.Where(x => x.Student_Course_Registration.Course_Section_Faculty.Id == data).OrderBy(x => x.Student_Course_Registration.Student.Email).ToList()
                    };
                }
                return View(viewDetails);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> GetAttendance(string data)
        {
            try
            {
                var attendanceLise = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
                attendanceLise = attendanceLise.Where(x => x.Student_Course_Registration.Course_Section_Faculty.Id == data).OrderBy(x => x.Student_Course_Registration.Student.Email).ToList();
                return View(attendanceLise);                            
            }
            catch (Exception)
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
        public async Task<IActionResult> StudentDashboard()
        {
            StudentDashboardViewModel studentDashboardViewModel = new();
            ProfileViewModel profile = new()
            {
                UserId = HttpContext.Session.GetString("UserId"),
                Email = HttpContext.Session.GetString("UserEmail"),
                Name = HttpContext.Session.GetString("UserName"),
                UserType = HttpContext.Session.GetString("_UserType"),
                Address = "N/A",
                PhoneNumber = "N/A"
            };
            studentDashboardViewModel.Profile = profile;
            var registrationList = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
            var studentEmail = HttpContext.Session.GetString("UserEmail");
            var registrationCourse = registrationList.Where(x => x.Student.Email.Equals(studentEmail, StringComparison.OrdinalIgnoreCase)).ToList();
            studentDashboardViewModel.TiltesData = new List<TilesViewModel>();
            foreach (var item in registrationCourse)
            {
                TilesViewModel tilesViewModel = new()
                {
                    Id = item.Id,
                    DisplayContent = item.Course_Section_Faculty.Course.Name,
                    Action = "GetRegisteredStudents",
                    Controller = "AMS",
                    Description = "Section :" + item.Course_Section_Faculty.Section.Name
                };
                studentDashboardViewModel.TiltesData.Add(tilesViewModel);
            }


            return View(studentDashboardViewModel);
        }

        public async Task<IActionResult> GetRegisteredStudents(string data)
        {
            ViewBag.IsPeopleEnable = data;
            var studnetCourseReg = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
            studnetCourseReg = studnetCourseReg.Where(x => x.Course_Section_Faculty.Id == data).ToList();
            return View(studnetCourseReg);
        }

        public async Task<IActionResult> StudentRegistration()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var scheduledCourses = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
            var courseRegistered = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
            courseRegistered = courseRegistered.Where(x => x.Student.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).ToList();
            IList<StudentRegistrationViewModel> studentRegistrationViewModel = new List<StudentRegistrationViewModel>();
            foreach (var sCourse in scheduledCourses)
            {
                var data = courseRegistered.FirstOrDefault(x => x.Course_Section_Faculty.Id.Equals(sCourse.Id));
                //var data = courseRegistered.FirstOrDefault(x => x.Course_Section_Faculty.Course.Id.Equals(sCourse.Course.Id, StringComparison.OrdinalIgnoreCase) && x.Course_Section_Faculty.Faculty.Id.Equals(sCourse.Faculty.Id, StringComparison.OrdinalIgnoreCase) && x.Course_Section_Faculty.Section.Id.Equals(sCourse.Section.Id, StringComparison.OrdinalIgnoreCase));
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
                        IsRegistered = true,
                        IsApproved = data.IsApproved
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
                        student_Course_Registration.IsApproved = false;
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
                var studentAttendance = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
                var studentCourseReg = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
                if (studentCourseReg != null && studentCourseReg.Count > 0)
                {
                    var id = studentCourseReg.FirstOrDefault(x => x.Id.Equals(data, StringComparison.OrdinalIgnoreCase))?.Id;
                    if (studentAttendance != null && studentAttendance.Count > 0)
                    {
                        studentAttendance = studentAttendance.Where(x => x.Student_Course_Registration.Id.Equals(id)).ToList();
                        foreach (var attendance in studentAttendance)
                        {
                            var attendanceResult = await dbOperations.DeleteData(attendance.Id, "Students_Attendance");
                        }
                    }
                    var result = await dbOperations.DeleteData(data, "Student_Course_Registration");
                    if (!result)
                    {
                        //Message with Error
                        return RedirectToAction("StudentRegistration");
                    }
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

        public async Task<string> GetAttendancePercentage(string studentEmail, string Uid)
        {

            var studentCourseReg = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
            if (studentCourseReg.Count > 0)
            {
                var currentStudentCourse = studentCourseReg.FirstOrDefault(x => x.Student.Email.Equals(studentEmail, StringComparison.OrdinalIgnoreCase) && x.Course_Section_Faculty.Id == Uid);
                if (currentStudentCourse != null)
                {
                    var totalCount = currentStudentCourse.Course_Section_Faculty.TotalCount;
                    var attendanceList = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
                    attendanceList = attendanceList.Where(x => x.Student_Course_Registration.Course_Section_Faculty.Id.Equals(currentStudentCourse.Course_Section_Faculty.Id) && x.Student_Course_Registration.Student.Email.Equals(studentEmail, StringComparison.OrdinalIgnoreCase) && x.IsApproved).ToList();
                    var percentage = (int)Math.Round((double)(100 * attendanceList.Count) / totalCount);
                    return percentage.ToString();
                }
            }

            return "";
        }

        [HttpGet]
        public async Task<IActionResult> ShowMyRegCourse()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var studentCourseReg = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
            studentCourseReg = studentCourseReg.Where(x => x.Student.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && x.IsApproved).ToList();
            var courseReg = new List<SelectListItem> { };
            foreach (var item in studentCourseReg)
            {
                var courseName = item.Course_Section_Faculty.Course.Name + "-" + item.Course_Section_Faculty.Section.Name + "-" + item.Course_Section_Faculty.Faculty.Name;
                SelectListItem selectList = new()
                {
                    Text = courseName,
                    Value = item.Id,
                    Selected = false
                };
                if (courseReg.Count < 0)
                {
                    selectList.Selected = true;
                }
                courseReg.Add(selectList);
            }
            ViewBag.CourseData = courseReg;
            return View();
        }

        public async Task<JsonResult> GetGraphDetails(string Uid)
        {

            if (!string.IsNullOrEmpty(Uid))
            {
                var studentCourseRegit = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
                if (studentCourseRegit.Count <= 0)
                {
                    return Json(string.Empty);
                }
                var currentStudentSub = studentCourseRegit.FirstOrDefault(x => x.Id.Equals(Uid, StringComparison.OrdinalIgnoreCase));
                if (currentStudentSub == null)
                {
                    return Json(string.Empty);
                }
                //Need to get Total from below list of Course_Section_Faculty
                var courserSectionFaculty = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
                if (courserSectionFaculty.Count <= 0)
                {
                    return Json(string.Empty);
                }
                var currentcourserSectionFaculty = courserSectionFaculty.FirstOrDefault(x => x.Id == currentStudentSub.Course_Section_Faculty.Id);
                if (currentcourserSectionFaculty == null)
                {
                    return Json(string.Empty);
                }
                if (courserSectionFaculty.Count > 0 && currentStudentSub.Course_Section_Faculty != null)
                {
                    var totalCount = currentcourserSectionFaculty.TotalCount;
                    var studentsAttendance = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
                    var attendedCount = studentsAttendance.Where(x => x.Student_Course_Registration.Course_Section_Faculty.Id.Equals(currentStudentSub.Course_Section_Faculty.Id, StringComparison.OrdinalIgnoreCase) && x.Student_Course_Registration.Student.Email.Equals(currentStudentSub.Student.Email, StringComparison.OrdinalIgnoreCase) && x.IsApproved).Count();
                    Dictionary<string, dynamic> keyValuePairs = new Dictionary<string, dynamic>();
                    int percentage = 0;
                    int absentCount = 0;
                    if (totalCount >= attendedCount)
                    {
                        absentCount = totalCount - attendedCount;
                        percentage = (int)Math.Round((double)(100 * attendedCount) / totalCount);
                        if (percentage < 0)
                        {
                            percentage = 0;
                        }
                    }

                    keyValuePairs.Add("totalCount", totalCount);
                    keyValuePairs.Add("attendedCount", attendedCount);
                    keyValuePairs.Add("absentCount", absentCount);
                    if (totalCount <= 0)
                    {
                        keyValuePairs.Add("percentage", "Class not yet started");
                    }
                    else
                    {
                        keyValuePairs.Add("percentage", percentage.ToString() + "%");
                    }
                    return Json(keyValuePairs);
                }
            }
            return Json(string.Empty);
        }

        public async Task<IActionResult> ViewMyAttendance(string uId)
        {
            if (uId == null || uId == "0")
            {
                var email = HttpContext.Session.GetString("UserEmail");
                var attendanceListTemp = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
                attendanceListTemp = attendanceListTemp.Where(x => x.Student_Course_Registration.Student.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).ToList();
                return View(attendanceListTemp);
            }
            var attendanceList = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
            attendanceList = attendanceList.Where(x => x.Student_Course_Registration.Id.Equals(uId, StringComparison.OrdinalIgnoreCase)).ToList();
            return View(attendanceList);
        }

        [AllowAnonymous]
        public IActionResult MarkAttendance(string uid, string QRId)
        {
            ViewBag.CId = uid;
            ViewBag.QRId = QRId;
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> SubmitAttendance(SubmitAttendanceViewModel data)
        {
            try
            {
                //var pinList = await dbOperations.GetAllData<UPIN>("UPIN");
                var QRList = await dbOperations.GetAllData<QRCodeTracker>("QRCodeTracker");
                var currentQRCode = QRList.FirstOrDefault(x => x.UId.Equals(data.CId, StringComparison.OrdinalIgnoreCase));
                if (currentQRCode == null || !currentQRCode.QRId.Equals(data.QRId, StringComparison.OrdinalIgnoreCase))
                {
                    ViewData["Invalid"] = "Invalid Request";
                    return View();
                }
                var studentList = await dbOperations.GetAllData<Student>("Student");
                if (!studentList.Any(x => x.Email.Equals(data.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    ViewData["Invalid"] = "Student does not exist";
                    return View();
                }
                //var userPINDetails = pinList.FirstOrDefault(x => x.Email.Equals(data.Email, StringComparison.OrdinalIgnoreCase));
                //if (userPINDetails.PIN != data.PIN)
                //{
                //    ViewData["Invalid"] = "Invalid PIN";
                //    return View();
                //}
                var student_Course_Registration = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
                var studentCourseRegistered = student_Course_Registration.FirstOrDefault(x => x.Student.Email.Equals(data.Email, StringComparison.OrdinalIgnoreCase) && x.Course_Section_Faculty.Id.Equals(data.CId, StringComparison.OrdinalIgnoreCase));
                if (studentCourseRegistered != null)
                {
                    var attendance = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
                    if (attendance.Where(x => x.AttendedOn.Date == DateTime.UtcNow.Date && x.Student_Course_Registration.Student.Email.Equals(data.Email, StringComparison.OrdinalIgnoreCase) && x.Student_Course_Registration.Course_Section_Faculty.Id.Equals(data.CId, StringComparison.OrdinalIgnoreCase)).Any())
                    {
                        ViewData["Invalid"] = "Already attendance marked for the class";
                        return View();
                    }
                    var result = await dbOperations.SaveData<Students_Attendance>(new Students_Attendance
                    {
                        AttendedOn = DateTime.UtcNow,
                        IsApproved = true,
                        IsAttended = true,
                        Student_Course_Registration = studentCourseRegistered
                    }, "Students_Attendance");
                    if (result != null)
                    {
                        ViewData["Valid"] = "Submitted";
                        return View();
                    }
                }
                else
                {
                    ViewData["Invalid"] = "Not Registered for the Course";
                    return View();
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
                currentAttendance.ApprovedOn = DateTime.UtcNow;
                var updatedData = await dbOperations.UpdateData<Students_Attendance>(data, currentAttendance, "Students_Attendance");
                return RedirectToAction("ViewRegCourseDetails", "AMS", new { data = cId });
            }
            return RedirectToAction("ViewRegCourseDetails", "AMS", new { data = cId });
        }

        #endregion Attendance

        #region QRCode

        [HttpGet]
        public async Task<IActionResult> CreateQRCode(string data)
        {
            ViewBag.IsVisible = true;
            var QId = string.Empty;
            var QRInformation = await dbOperations.GetAllData<QRCodeTracker>("QRCodeTracker");
            if (QRInformation != null && QRInformation.Any(x => x.UId.Equals(data, StringComparison.OrdinalIgnoreCase)))
            {
                var currentRecord = QRInformation.FirstOrDefault(x => x.UId.Equals(data, StringComparison.OrdinalIgnoreCase));
                if (currentRecord != null)
                {
                    currentRecord.QRId = Guid.NewGuid().ToString("N");
                    QId = currentRecord.QRId;
                    ViewBag.QRExpiresIn = currentRecord.Expiry.ToString("MMM d yyyy HH:mm:ss");
                    if ((currentRecord.Expiry < DateTime.UtcNow))
                    {
                        ViewBag.IsVisible = false;
                    }
                    await dbOperations.UpdateData<QRCodeTracker>(currentRecord.Id, currentRecord, "QRCodeTracker");
                }
            }
            else
            {
                QRCodeTracker qRCodeTracker = new QRCodeTracker
                {
                    Expiry = DateTime.UtcNow.AddMinutes(5),
                    QRId = Guid.NewGuid().ToString("N"),
                    UId = data
                };
                ViewBag.QRExpiresIn = qRCodeTracker.Expiry.ToString("MMM d yyyy HH:mm:ss");
                QId = qRCodeTracker.QRId;
                await dbOperations.SaveData<QRCodeTracker>(qRCodeTracker, "QRCodeTracker");
            }
            await AddClassCount(data);
            var url = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "/AMS/MarkAttendance?uid=" + data + "&QRId=" + QId;
            string WebUri = new Uri(url).ToString();
            string UriPayload = WebUri.ToString();
            QRCodeGenerator QrGenerator = new();
            QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(UriPayload, QRCodeGenerator.ECCLevel.Q);
            QRCode QrCode = new QRCode(QrCodeInfo);
            Bitmap QrBitmap = QrCode.GetGraphic(60);
            byte[] BitmapArray = QrBitmap.BitmapToByteArray();
            string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
            ViewBag.QrCodeUri = QrUri;
            ViewBag.CurrentTime = DateTime.UtcNow;
            ViewBag.QrCodeId = data;

            return View();
        }

        public async Task<IActionResult> ExtendQRCodeTime(string data)
        {
            var QRInformation = await dbOperations.GetAllData<QRCodeTracker>("QRCodeTracker");
            if (QRInformation != null && QRInformation.Any(x => x.UId.Equals(data, StringComparison.OrdinalIgnoreCase)))
            {
                var currentRecord = QRInformation.FirstOrDefault(x => x.UId.Equals(data, StringComparison.OrdinalIgnoreCase));
                if (currentRecord != null)
                {
                    currentRecord.Expiry = DateTime.UtcNow.AddMinutes(5);
                    await dbOperations.UpdateData<QRCodeTracker>(currentRecord.Id, currentRecord, "QRCodeTracker");
                }
            }
            return RedirectToAction("ViewRegCourseDetails", "AMS", new { data = data });
        }

        #endregion QRCode

        #region MyProfile

        public IActionResult MyProfile()
        {

            ProfileViewModel profile = new ProfileViewModel
            {
                UserId = HttpContext.Session.GetString("UserId"),
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
                    TempData["IsPINSet"] = true;
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
                    TempData["IsPINSet"] = true;
                    return RedirectToAction("MyProfile", "AMS");
                }
            }
            TempData["IsPINSet"] = false;
            return RedirectToAction("MyProfile", "AMS");
        }

        private async Task<bool> AddClassCount(string uId)
        {
            var data = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
            if (data != null && data.Count > 0)
            {
                var currentRecord = data.FirstOrDefault(x => x.Id.Equals(uId, StringComparison.OrdinalIgnoreCase));
                if (currentRecord != null && currentRecord.LastClassOn.Date < DateTime.UtcNow.Date)
                {
                    currentRecord.TotalCount += 1;
                    currentRecord.LastClassOn = DateTime.UtcNow;
                    var result = await dbOperations.UpdateData<Course_Section_Faculty>(currentRecord.Id, currentRecord, "Course_Section_Faculty");
                    if (result != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion MyProfile

        #region AdminApprovals

        public async Task<IActionResult> AdminStudentRegistration()
        {
            var students = await dbOperations.GetAllData<Student>("Student");
            var scheduledCourses = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
            var registrationList = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
            AdminStudentRegistrationViewModel studentRegistrationViewModel = new()
            {
                Students = students,
                Course_Section_Faculty = scheduledCourses,
                RegistrationList = registrationList
            };
            return View(studentRegistrationViewModel);
        }

        public async Task<IActionResult> AdminFacultyRegistration()
        {
            var facultList = await dbOperations.GetAllData<Faculty>("Faculty");
            var courseList = await dbOperations.GetAllData<Course>("Course");
            var sectionList = await dbOperations.GetAllData<Section>("Section");
            var registrationList = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
            FacultyRegistrationViewModel data = new()
            {
                Faculties = facultList,
                Courses = courseList,
                Sections = sectionList,
                RegistrationList = registrationList.ToList()
            };
            return View(data);
        }

        public async Task<IActionResult> AdminRegisterFacultyWithCourse(CourseViewModel courseViewModel)
        {
            try
            {
                if (courseViewModel.FName == null || courseViewModel == null || courseViewModel.SName == null || courseViewModel.CName == null)
                {
                    //With Fail Message
                    return View("AdminFacultyRegistration");
                }
                Course_Section_Faculty data = new() { };
                var courseList = await dbOperations.GetAllData<Course>("Course");
                var sectionList = await dbOperations.GetAllData<Section>("Section");
                var facultyList = await dbOperations.GetAllData<Faculty>("Faculty");
                if (courseList != null && courseList.Count > 0 && sectionList != null && sectionList.Count > 0 && facultyList != null && facultyList.Count > 0)
                {
                    var facultyInfo = facultyList.FirstOrDefault(x => x.Email == courseViewModel.FName);
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
                    return View("AdminFacultyRegistration");
                }
                var result = await dbOperations.SaveData(data, "Course_Section_Faculty");
                if (result == null)
                {
                    //With Success Message
                    return RedirectToAction("AdminFacultyRegistration");
                }
                return RedirectToAction("AdminFacultyRegistration");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> AdminRegisterStudentWithCourse(CourseViewModel courseViewModel)
        {
            try
            {
                if (courseViewModel == null)
                {
                    //Fail Message
                    return RedirectToAction("AdminStudentRegistration", "AMS");
                }
                Student_Course_Registration student_Course_Registration = new Student_Course_Registration();
                var course_Section_Faculty = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
                var studentList = await dbOperations.GetAllData<Student>("Student");
                var selectedCourse_Section_Faculty = course_Section_Faculty.FirstOrDefault(x => x.Id == courseViewModel.CName);
                if (selectedCourse_Section_Faculty != null && studentList.Count > 0)
                {
                    var student = studentList.FirstOrDefault(x => x.Email.Equals(courseViewModel.SName, StringComparison.OrdinalIgnoreCase));
                    if (student != null)
                    {
                        student_Course_Registration.Course_Section_Faculty = selectedCourse_Section_Faculty;
                        student_Course_Registration.Student = student;
                        student_Course_Registration.IsApproved = false;
                    }
                }
                if (student_Course_Registration.Student == null || student_Course_Registration.Course_Section_Faculty == null)
                {
                    //Message with Error
                    return RedirectToAction("AdminStudentRegistration");
                }
                var result = await dbOperations.SaveData(student_Course_Registration, "Student_Course_Registration");
                if (result == null)
                {
                    //Message with Error
                    return RedirectToAction("AdminStudentRegistration");
                }
                return RedirectToAction("AdminStudentRegistration");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IActionResult> GetPendingUserApprovals()
        {
            var usersList = await dbOperations.GetAllData<Models.User>("User");
            usersList = usersList.Where(x => x.IsApproved == false).ToList();
            return View("PendingUserApprovals", usersList);
        }

        public async Task<IActionResult> UpdateUserStatus(string userEmail)
        {
            var usersList = await dbOperations.GetAllData<Models.User>("User");
            var currentUser = usersList.FirstOrDefault(x => x.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase));
            if (currentUser != null)
            {
                currentUser.IsApproved = true;
                var result = await dbOperations.UpdateData<Models.User>(currentUser.Id, currentUser, "User");
            }
            return RedirectToAction("GetPendingUserApprovals");
        }

        public async Task<IActionResult> GetPendingSubjectApprovals()
        {
            var usersList = await dbOperations.GetAllData<Models.Student_Course_Registration>("Student_Course_Registration");
            usersList = usersList.Where(x => x.IsApproved == false).ToList();
            return View("PendingSubjectApprovals", usersList);
        }

        public async Task<IActionResult> UpdateStudentCourseStatus(string id)
        {
            var studentCourseList = await dbOperations.GetAllData<Models.Student_Course_Registration>("Student_Course_Registration");
            var currentStudentCourse = studentCourseList.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (currentStudentCourse != null)
            {
                currentStudentCourse.IsApproved = true;
                var result = await dbOperations.UpdateData<Models.Student_Course_Registration>(currentStudentCourse.Id, currentStudentCourse, "Student_Course_Registration");
            }
            return RedirectToAction("GetPendingSubjectApprovals");
        }

        #endregion AdminApprovals

        #region errorPages
        public IActionResult Error()
        {
            return View();
        }

        #endregion errorPages
    }
}
