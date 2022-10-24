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
        /// Shows Registration page 
        /// </summary>
        /// <returns></returns>
        public IActionResult Register()
        {
            ViewData["UserName"] = GenerateUID();
            return View();
        }

        /// <summary>
        /// Process the Registration 
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RegisterUser(Models.User userModel)
        {
            try
            {
                var userList = await dbOperations.GetAllData<Models.User>("User");
                if (userList.Count > 0)
                {
                    var currentUser = userList.FirstOrDefault(x => x.Email.Equals(userModel.Email, StringComparison.OrdinalIgnoreCase));
                    if (currentUser != null)
                    {
                        ViewData["Invalid"] = "Email Already Exist..!";
                        ViewData["UserName"] = GenerateUID();
                        return View("Register");
                    }
                }
                //Talks with Firebase Auth process and creates the user with provided userId and password.
                var regResult = await AuthProvider.CreateUserWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
                if (regResult == null || regResult?.FirebaseToken == null)
                {
                    // If something went wrong on firebase then we wil show below message
                    ViewData["Invalid"] = "Some thing went wrong..!";
                    ViewData["UserName"] = GenerateUID();
                    return View("Register");
                }
                userModel.Password = string.Empty;
                userModel.IsApproved = false;
                var user = await dbOperations.SaveData(userModel, "User");
                if (user == null)
                {
                    ViewData["Invalid"] = "Something went wrong..!";
                    ViewData["UserName"] = GenerateUID();
                    return View("Register");
                }
                //Creating the student model to be saved in database
                if (userModel.Type == "Student")
                {
                    var student = new Student
                    {
                        Email = userModel.Email,
                        FirstName = userModel.FirstName,
                        LastName = userModel.LastName,
                        Name = userModel.UserName
                    };
                    var uPIN = new UPIN
                    {
                        Email = userModel.Email,
                        PIN = int.Parse(GenerateUID())
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
                        FirstName = userModel.FirstName,
                        LastName = userModel.LastName,
                        Name = userModel.UserName
                    };

                    var result = await dbOperations.SaveData(faculty, "Faculty");
                }

                ViewData["Valid"] = "Registered Successfully.";
                ViewData["UserName"] = GenerateUID();
                return View("Register");
            }

            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(String.Empty, firebaseEx.error.message);
                ViewData["Invalid"] = firebaseEx.error.message;
                ViewData["UserName"] = GenerateUID();
                return View("Register");
            }

        }

        /// <summary>
        /// Shows Login page 
        /// </summary>
        /// <returns></returns>
        public IActionResult SignIn()
        {
            return View();
        }

        /// <summary>
        /// Process the Login
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
                    var userList = await dbOperations.GetAllData<Models.User>("User");
                    var currentUser = userList.FirstOrDefault(x => x.Email.Equals(userModel.Email, StringComparison.OrdinalIgnoreCase));
                    if (currentUser == null)
                    {
                        ViewData["Invalid"] = "Fail to login";
                        return View("SignIn");
                    }
                    if (!currentUser.IsApproved)
                    {
                        ViewData["Invalid"] = "User Not Approved";
                        return View("SignIn");
                    }
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userModel.Email),
                        new Claim("_UserToken", token),
                        new Claim(ClaimTypes.Role, currentUser.Type),
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(25),
                        IssuedUtc = DateTime.UtcNow,
                        RedirectUri = Url.Action("Index", "HomePage")
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    HttpContext.Session.SetString("_UserToken", token);
                    HttpContext.Session.SetString("UserName", currentUser.FirstName + ' ' + currentUser.LastName ?? "");
                    HttpContext.Session.SetString("UserEmail", userModel.Email ?? "");
                    HttpContext.Session.SetString("UserType", currentUser.Type);
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
                ModelState.AddModelError(string.Empty, firebaseEx.error.message);
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

        /// <summary>
        /// Reset Password
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ResetPassword()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email != null)
            {
                await AuthProvider.SendPasswordResetEmailAsync(email);
                TempData["IsPasswordSet"] = true;
                return RedirectToAction("MyProfile", "AMS");
            }
            TempData["IsPasswordSet"] = false;
            return RedirectToAction("MyProfile", "AMS");

        }

        /// <summary>
        /// FogotPasword
        /// </summary>
        /// <returns></returns>
        public IActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }

        /// <summary>
        /// ForgotmPassword
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                await AuthProvider.SendPasswordResetEmailAsync(email);
                TempData["IsPasswordSet"] = true;
                return RedirectToAction("ForgotPassword");
            }
            catch (Exception ex)
            {
                TempData["IsPasswordSet"] = false;
                return RedirectToAction("ForgotPassword");
            }

        }

        private string GenerateUID()
        {
            Random generator = new Random();
            String r = generator.Next(0, 1000000).ToString("D6");
            return r;
        }
    }
}
