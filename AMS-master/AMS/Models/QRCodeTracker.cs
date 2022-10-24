namespace AMS.Models
{
    public class QRCodeTracker : BaseModel
    {
        public string UId { get; set; }
        public string QRId { get; set; }
        public DateTime Expiry { get; set; }
    }
}
