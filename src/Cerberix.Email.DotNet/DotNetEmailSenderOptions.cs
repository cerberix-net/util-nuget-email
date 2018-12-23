namespace Cerberix.Email.DotNet
{
    public class DotNetEmailSenderOptions
    {
        public string UserName { get; }
        public string Password { get; }
        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public int MaxDegreeOfParallelism { get; }

        public DotNetEmailSenderOptions(
            string userName,
            string password,
            string host,
            int port,
            bool enableSsl,
            int maxDegreeOfParallelism
            ) 
        {
            UserName = userName;
            Password = password;
            Host = host;
            Port = port;
            EnableSsl = enableSsl;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }
    }
}
