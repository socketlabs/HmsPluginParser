using System.Collections.Generic;
using MimeKit;

namespace HmsPluginParser.Parsing
{
    public class MessageDetails
    {
        public List<MimePart> Attachments { get; set; }
        public InternetAddressList To { get; set; }
        public InternetAddressList Cc { get; set; }
        public InternetAddressList Bcc { get; set; }
        public HeaderList Headers { get; set; }
        public MimeMessage Message { get; set; }
    }
}
