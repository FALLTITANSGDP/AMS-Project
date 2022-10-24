using System.Reflection.Metadata.Ecma335;

namespace AMS.ViewModels
{
    public class EmailModel
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public bool IsHtmlBody { get; set; }
    }
}
