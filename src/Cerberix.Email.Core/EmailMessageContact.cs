namespace Cerberix.Email.Core
{
    public class EmailMessageContact
    {
        public string Email { get; }
        public string Name { get; }

        public EmailMessageContact(
            string email,
            string name
            )
        {
            Email = email;
            Name = name;
        }
    }
}
