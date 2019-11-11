using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cerberix.Email
{
    public interface IEmailSender
    {
        Task<IReadOnlyCollection<KeyValuePair<EmailMessage, EmailSenderResponse>>> Send(
            IReadOnlyCollection<EmailMessage> messages
            );
    }
}
