using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cerberix.Email.Core;
using Cerberix.Logging.Core;

namespace Cerberix.Email.DotNet.Logic
{
    internal class DotNetEmailSender : IEmailSender
    {
        private readonly ILogSink _logger;
        private readonly DotNetEmailSenderOptions _options;

        public DotNetEmailSender(
            ILogSink logger,
            DotNetEmailSenderOptions options
            )
        {
            ValidateLogger(logger);
            ValidateOptions(options);

            _logger = logger;
            _options = options;
        }

        public Task<IReadOnlyCollection<KeyValuePair<EmailMessage, EmailSenderResponse>>> Send(
            IReadOnlyCollection<EmailMessage> messages
            )
        {
            var responses = new ConcurrentBag<KeyValuePair<EmailMessage, EmailSenderResponse>>();

            var parallelOptions = GetParallelOptions(_options.MaxDegreeOfParallelism);
            Parallel.ForEach(messages, parallelOptions, (EmailMessage message) =>
            {
                try
                {
                    SendCore(_options, message);

                    var response = new KeyValuePair<EmailMessage, EmailSenderResponse>(
                        key: message, 
                        value: new EmailSenderResponse(EmailSenderResponseStatusCode.OK)
                        );
                    responses.Add(response);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogSeverity.Error, "An unexpected error occurred.", ex);

                    var response = new KeyValuePair<EmailMessage, EmailSenderResponse>(
                        key: message,
                        value: new EmailSenderResponse(EmailSenderResponseStatusCode.InternalServerError)
                        );
                    responses.Add(response);
                }
            });

            return Task.FromResult<IReadOnlyCollection<KeyValuePair<EmailMessage, EmailSenderResponse>>>(responses);
        }

        //
        //  Smtp
        //

        private static ParallelOptions GetParallelOptions(int maxDegreeOfParallelism)
        {
            return new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };
        }

        private static MailAddress GetMailAddress(EmailMessageContact contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            var result = !string.IsNullOrWhiteSpace(contact.Name)
                ? new MailAddress(contact.Email, contact.Name)
                : new MailAddress(contact.Email);
            return result;
        }

        private static MailMessage GetMailMessage(EmailMessage incoming)
        {
            if (incoming == null)
            {
                throw new ArgumentNullException("incoming");
            }

            // Create a System.Net.Mail.MailMessage object
            var message = new MailMessage();

            // Add a message subject
            message.Subject = incoming.Subject;

            // Add a message body
            message.Body = incoming.Body;

            // Create a System.Net.Mail.MailAddress object and 
            // set the sender email address and display name
            message.From = GetMailAddress(incoming.From);

            // add optional reply-to
            if (incoming.ReplyTo != null)
            {
                var replyTo = GetMailAddress(incoming.ReplyTo);
                message.ReplyToList.Add(replyTo);
            }

            // Add recipient(s)
            foreach (var to in incoming.To)
            {
                message.To.Add(GetMailAddress(to));
            }

            // optionally add CC recipient(s)
            if (incoming.CC != null && incoming.CC.Any())
            {
                foreach (var cc in incoming.CC)
                {
                    message.CC.Add(GetMailAddress(cc));
                }
            }

            // optionally add BCC recipient(s)
            if (incoming.Bcc != null && incoming.Bcc.Any())
            {
                foreach (var bcc in incoming.Bcc)
                {
                    message.Bcc.Add(GetMailAddress(bcc));
                }
            }

            return message;
        }

        private static SmtpClient GetSmtpClient(
            DotNetEmailSenderOptions options
            )
        {
            // Create a System.Net.Mail.SmtpClient object
            // and set the SMTP host and port number
            var smtp = new SmtpClient(options.Host, options.Port);

            // If your server requires authentication add the below code
            // =========================================================
            // Enable Secure Socket Layer (SSL) for connection encryption
            smtp.EnableSsl = options.EnableSsl;

            // Do not send the DefaultCredentials with requests
            // Create a System.Net.NetworkCredential object and set
            // the username and password required by your SMTP account
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(options.UserName, options.Password);
            // =========================================================

            return smtp;
        }

        private static void SendCore(
            DotNetEmailSenderOptions options,
            EmailMessage incoming
            )
        {
            // compose message from incoming spec
            var message = GetMailMessage(incoming);

            // compose smtp client from options
            var smtp = GetSmtpClient(options);

            // Send the message (sync)
            smtp.Send(message);
        }

        //
        //  Validation
        //

        private static bool IsValidEmailAddress(string emailAddress)
        {
            try
            {
                return Regex.IsMatch(emailAddress,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private static void ValidateContact(EmailMessageContact contact)
        {
            // throw when null
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            // validate proper email address provided
            var email = contact.Email;
            var isValid = IsValidEmailAddress(email);
            if (!isValid)
            {
                throw new ArgumentException("{Email} is not a valid email address.", "email");
            }

            //
            // NOTE: name is optional => no validation performed
            //
        }

        private static void ValidateLogger(ILogSink logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
        }

        private static void ValidateOptions(DotNetEmailSenderOptions options)
        {
            // throw when null
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            // throw when null/empty
            if (string.IsNullOrWhiteSpace(options.UserName))
            {
                throw new ArgumentException("{UserName} cannot be null or empty.", "credentials");
            }

            // throw when null/empty
            if (string.IsNullOrWhiteSpace(options.Password))
            {
                throw new ArgumentException("{Password} cannot be null or empty.", "credentials");
            }

            // throw when null/empty
            if (string.IsNullOrWhiteSpace(options.Host))
            {
                throw new ArgumentException("{Host} cannot be null or empty.", "options");
            }

            // throw when unpexected range 
            if (options.MaxDegreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException("options", "{MaxDegreeOfParallelism} must be positive integer.");
            }

            // throw when unpexected range 
            if (options.Port <= 0)
            {
                throw new ArgumentOutOfRangeException("options", "{Port} must be positive integer.");
            }
        }

        private static void ValidateFileAttachment(EmailMessageAttachment attachment)
        {
            // throw when null
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            // throw when null
            if (attachment.Bytes == null)
            {
                throw new ArgumentNullException("attachment", "{Bytes} cannot be null.");
            }

            // throw when null/empty
            if (string.IsNullOrWhiteSpace(attachment.FileName))
            {
                throw new ArgumentException("{FileName} cannot be null or empty.", "attachment");
            }

            // throw when null/empty
            if (string.IsNullOrWhiteSpace(attachment.MimeType))
            {
                throw new ArgumentException("{MimeType} cannot be null or empty.", "attachment");
            }
        }

        private static void ValidateMessage(EmailMessage message)
        {
            // throw when null
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            // validate contact
            ValidateContact(message.From);

            // throw when null/empty
            if (string.IsNullOrWhiteSpace(message.Subject))
            {
                throw new ArgumentException("{Subject} cannot be null or empty.", "message");
            }

            // throw when null/empty
            if (string.IsNullOrWhiteSpace(message.Body))
            {
                throw new ArgumentException("{Body} cannot be null or empty.", "message");
            }

            // throw when null
            if (message.To == null)
            {
                throw new ArgumentNullException("message", "{To} cannot be null.");
            }

            // validate each contact
            foreach (var to in message.To)
            {
                ValidateContact(to);
            }

            // optional reply to validatio
            if (message.ReplyTo != null)
            {
                ValidateContact(message.ReplyTo);
            }

            // optional CC validation
            if (message.CC != null && message.CC.Any())
            {
                // validate each contact
                foreach (var cc in message.CC)
                    ValidateContact(cc);
            }

            // optional BCC validation
            if (message.Bcc != null && message.Bcc.Any())
            {
                // validate each contact
                foreach (var bcc in message.Bcc)
                {
                    ValidateContact(bcc);
                }
            }

            // optional attachment validation
            if (message.Attachments != null && message.Attachments.Any())
            {
                // validate each attachment in container
                foreach (var attachment in message.Attachments)
                {
                    ValidateFileAttachment(attachment);
                }
            }
        }

        private static void ValidateMessages(IReadOnlyCollection<EmailMessage> messages)
        {
            // throw when null
            if (messages == null)
            {
                throw new ArgumentNullException("messages");
            }

            // throw when empty
            if (!messages.Any())
            {
                throw new ArgumentException("messages cannot be empty.");
            }


            // validate each message in container
            foreach (var message in messages)
            {
                ValidateMessage(message);
            }
        }
    }
}