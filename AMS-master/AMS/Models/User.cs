using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class User: BaseModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
