using System.Collections.Generic;
using System.IO;
using MimeKit;

namespace HmsPluginParser.Parsing
{
    public class Parsers
    {
        /// <summary>
        /// Parses the RCPTO TO (SMTP) recipients passed in by the MTA.
        /// </summary>
        /// <param name="delimitedRecipients"></param>
        /// <returns></returns>
        public static IEnumerable<RecipientAddress> ParseRecipientList(string delimitedRecipients)
        {
            List<string> recipientList = new List<string>();
            var recipients = delimitedRecipients.Split(',');
            foreach (var recipient in recipients)
            {
                char[] removesThese = { '<', '>', ' ' };
                var fullAddress = recipient.Trim(removesThese);
                var parts = fullAddress.Split('@');
                yield return new RecipientAddress()
                    {Email = fullAddress, Domain = parts.Length == 2 ? parts[1] : parts[0], Friendly = parts[0]};
            }
        }

        /// <summary>
        /// Prases the message using the MimeKit package.
        /// </summary>
        /// <param name="messageStream"></param>
        /// <returns></returns>
        public static MessageDetails ParseMessage(Stream messageStream)
        {
            // Load a MimeMessage from a stream using MimeKit
            // See for more docs  https://github.com/jstedfast/MimeKit
            var message = MimeMessage.Load(messageStream);

            var attachments = new List<MimePart>();
            var multiparts = new List<Multipart>();
            var iter = new MimeIterator(message);

            // collect our list of attachments and their parent multi-parts
            while (iter.MoveNext())
            {
                if (iter.Parent is Multipart multipart && iter.Current is MimePart part && part.IsAttachment)
                {
                    // keep track of each attachment's parent multipart
                    multiparts.Add(multipart);
                    attachments.Add(part);
                }
            }

            // now remove each attachment from its parent multipart...
            for (int i = 0; i < attachments.Count; i++)
                multiparts[i].Remove(attachments[i]);

            var msg = new MessageDetails();
            msg.Attachments = attachments;
            msg.To = message.To;
            msg.Cc = message.Cc;
            msg.Bcc = message.Bcc;
            msg.Headers = message.Headers;
            msg.Message = message;
            return msg;

        }
    }
}
