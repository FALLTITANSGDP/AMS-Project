namespace AMS.Models
{
    public class UserBaseModel : BaseModel
    {
        public string DisplayName
        {
            get { return FirstName + "-" + LastName; }
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
