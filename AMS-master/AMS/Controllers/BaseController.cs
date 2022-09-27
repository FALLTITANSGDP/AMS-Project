using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers
{
    public class BaseController : Controller
    {
        public static string firebaseDatabaseUrl = "https://attendance-tracking-system-ft-default-rtdb.firebaseio.com";

        public string APIKey = "AIzaSyABqIyfQB8vfsfpMouZTceIzuxcyhTftlw";
        public FirebaseConfig AuthConfig { get; set; }
        public FirebaseAuthProvider AuthProvider { get; set; }
        public BaseController()
        {
            AuthProvider = new FirebaseAuthProvider(new FirebaseConfig(APIKey));
            AuthConfig = new FirebaseConfig(APIKey);
        }

    }
}
