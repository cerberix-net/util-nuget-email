using System.Collections.Generic;

namespace Cerberix.Email
{
    public class EmailMessage
    {
        public IReadOnlyCollection<EmailMessageContact> To { get; }
        public IReadOnlyCollection<EmailMessageContact> CC { get; }
        public IReadOnlyCollection<EmailMessageContact> Bcc { get; }
        public IReadOnlyCollection<EmailMessageAttachment> Attachments { get; }
        public EmailMessageContact From { get; }
        public EmailMessageContact ReplyTo { get; }
        public string Subject { get; }
        public string Body { get; }

        public EmailMessage(
            IReadOnlyCollection<EmailMessageContact> to,
            IReadOnlyCollection<EmailMessageContact> cc,
            IReadOnlyCollection<EmailMessageContact> bcc,
            IReadOnlyCollection<EmailMessageAttachment> attachments,
            EmailMessageContact from,
            EmailMessageContact replyTo,
            string subject,
            string body
            )
        {
            To = to;
            CC = cc;
            Bcc = bcc;
            Attachments = attachments;
            From = from;
            ReplyTo = replyTo;
            Subject = subject;
            Body = body;
        }
    }
}
