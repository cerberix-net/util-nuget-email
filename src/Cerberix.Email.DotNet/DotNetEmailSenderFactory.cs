using Cerberix.Email.Core;
using Cerberix.Logging.Core;

namespace Cerberix.Email.DotNet
{
    public static class DotNetEmailSenderFactory
    {
        public static IEmailSender GetInstance(
            ILogSink logger,
            DotNetEmailSenderOptions options
            )
        {
            return new Logic.DotNetEmailSender(
                logger: logger,
                options: options
                );
        }
    }
}
