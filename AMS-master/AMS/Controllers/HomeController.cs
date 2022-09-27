using AMS.Models;
using AMS.Services;
using AMS.ViewModels;
using Firebase.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using System.Security.Claims;
using static QRCoder.PayloadGenerator;

namespace AMS.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IDbOperations dbOperations;
        public HomeController(IDbOperations dbOperations)
        {
            this.dbOperations = dbOperations;
        }

        /// <summary>
        /// This page is shown after login of a faculty or student.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("_UserToken");
            if (token != null)
            {
                var email = HttpContext.Session.GetString("UserEmail");
                var pinDetails = await dbOperations.GetAllData<UPIN>("UPIN");
                if (pinDetails != null && pinDetails.Count > 0)
                {
                    var userPIN = pinDetails.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase))?.PIN ?? 0000;
                    HttpContext.Session.SetInt32("UserPIN", userPIN);
                }
                if (User.IsInRole("Student"))
                {
                    var courseReg = await dbOperations.GetAllData<Student_Course_Registration>("Student_Course_Registration");
                    ViewData["CourseReg"] = courseReg.Count(x => x.Student.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                    var courseAttended = await dbOperations.GetAllData<Students_Attendance>("Students_Attendance");
                    ViewData["CourseAtt"] = courseAttended.Count(x => x.Student_Course_Registration.Student.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                }
                if (User.IsInRole("Faculty"))
                {
                    var courseReg = await dbOperations.GetAllData<Course_Section_Faculty>("Course_Section_Faculty");
                    ViewData["CourseReg"] = courseReg.Count(x => x.Faculty.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                }
                return View();
            }
            else
            {
                return RedirectToAction("SignIn");
            }
        }

        /// <summary>
        /// Shows Registration page for the Student
        /// </summary>
        /// <returns></returns>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Process the Registration of a Student
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RegisterUser(Models.User userModel)
        {
            try
            {
                if (userModel.Type == "Student")
                {
                    var studentList = await dbOperations.GetAllData<Student>("Student");
                    if (studentList != null && studentList.Any(x => x.Email?.ToLower() == userModel.Email.ToLower()))
                    {
                        //User Already Exist
                        //Return to Registration Page and say that email aready exist.
                        ViewData["Invalid"] = "Email Already Exist..!";
                        return View("Register");
                    }
                }

                if (userModel.Type == "Faculty")
                {
                    var facultyList = await dbOperations.GetAllData<Faculty>("Faculty");
                    if (facultyList != null && facultyList.Any(x => x.Email?.ToLower() == userModel.Email.ToLower()))
                    {
                        //User Already Exist
                        ViewData["Invalid"] = "Email Already Exist..!";
                        return View("Register");

                    }
                }
                //Talks with Firebase Auth process and creates the user with provided userId and password.
                var regResult = await AuthProvider.CreateUserWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
                if (regResult == null || regResult?.FirebaseToken == null)
                {
                    // If something went wrong on firebase then we wil show below message
                    ViewData["Invalid"] = "Some thing went wrong..!";
                    return View("Register");
                }

                //Creating the student model to be saved in database
                if (userModel.Type == "Student")
                {
                    var student = new Student
                    {
                        Email = userModel.Email,
                        Name = userModel.UserName
                    };
                    var uPIN = new UPIN
                    {
                        Email = userModel.Email,
                        PIN = new Random().Next(0, 10000),
                    };

                    var upinResult = await dbOperations.SaveData<UPIN>(uPIN, "UPIN");
                    //Save the student in the student table.
                    var result = await dbOperations.SaveData(student, "Student");
                }
                if (userModel.Type == "Faculty")
                {
                    var faculty = new Faculty
                    {
                        Email = userModel.Email,
                        Name = userModel.UserName
                    };

                    var result = await dbOperations.SaveData(faculty, "Faculty");
                }

                ViewData["Valid"] = "Registered Successfully.";
                return View("Register");
            }

            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.error.message);
                ViewData["Invalid"] = "Some thing went wrong..!";
                return View("Register");
            }

        }

        /// <summary>
        /// Shows Login page for a student
        /// </summary>
        /// <returns></returns>
        public IActionResult SignIn()
        {
            return View();
        }

        /// <summary>
        /// Process the Login of a studnet
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SignInUser(Models.User userModel)
        {
            try
            {
                //log in the user
                var fbAuthLink = await AuthProvider
                                .SignInWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
                string token = fbAuthLink.FirebaseToken;
                //saving the token in a session variable
                if (token != null)
                {
                    var userName = string.Empty;
                    if (userModel.Type == "Student")
                    {
                        var studentList = await dbOperations.GetAllData<Student>("Student");
                        if (studentList != null && studentList.Any(x => x.Email?.ToLower() == userModel.Email.ToLower()))
                        {
                            userName = studentList.FirstOrDefault(x => x.Email?.ToLower() == userModel.Email.ToLower()).Name;
                        }
                        if (!studentList.Any(x => x.Email?.ToLower() == userModel.Email.ToLower()))
                        {
                            ViewData["Invalid"] = "Fail to login";
                            return View("SignIn");
                        }
                    }
                    if (userModel.Type == "Faculty")
                    {
                        var facultyList = await dbOperations.GetAllData<Faculty>("Faculty");
                        if (facultyList != null && facultyList.Any(x => x.Email?.ToLower() == userModel.Email.ToLower()))
                        {
                            userName = facultyList.FirstOrDefault(x => x.Email?.ToLower() == userModel.Email.ToLower()).Name;
                        }
                        if (!facultyList.Any(x => x.Email?.ToLower() == userModel.Email.ToLower()))
                        {
                            ViewData["Invalid"] = "Fail to login";
                            return View("SignIn");
                        }
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userModel.Email),
                        new Claim("_UserToken", token),
                        new Claim(ClaimTypes.Role, userModel.Type),
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(25),
                        IssuedUtc = DateTime.UtcNow,
                        RedirectUri = Url.Action("Index", "Home")
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    HttpContext.Session.SetString("_UserToken", token);
                    HttpContext.Session.SetString("UserName", userName ?? "");
                    HttpContext.Session.SetString("UserEmail", userModel.Email ?? "");
                    HttpContext.Session.SetString("UserType", userModel.Type);
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewData["Invalid"] = "Fail to login";
                    return View("SignIn");
                }
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.error.message);
                ViewData["Invalid"] = "Fail to login";
                return View("SignIn");
            }

        }



        /// <summary>
        /// Process the logout of a user (student/faculty)
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> LogOut()
        {
            HttpContext.Session.Remove("_UserToken");

            // Clear the existing external cookie
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("SignIn");
        }

        public async Task<IActionResult> ResetPassword()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email != null)
            {
                await AuthProvider.SendPasswordResetEmailAsync(email);
                HttpContext.Session.SetString("IsPasswordSet", "TRUE");
                return RedirectToAction("MyProfile", "AMS");
            }
            HttpContext.Session.SetString("IsPasswordSet", "FALSE");
            return RedirectToAction("MyProfile", "AMS");

        }

        public IActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                await AuthProvider.SendPasswordResetEmailAsync(email);
                HttpContext.Session.SetString("IsPasswordSet", "TRUE");
                return RedirectToAction("ForgotPassword");
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("IsPasswordSet", "FALSE");
                return RedirectToAction("ForgotPassword");
            }
           
        }
    }
}
