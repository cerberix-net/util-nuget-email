using System.Collections.Generic;

namespace Cerberix.Email
{
    public class EmailMessageAttachment
    {
        public IReadOnlyCollection<byte> Bytes { get; }
        public string FileName { get; }
        public string MimeType { get; }

        public EmailMessageAttachment(
            IReadOnlyCollection<byte> bytes,
            string fileName,
            string mimeType
            )
        {
            Bytes = bytes;
            FileName = fileName;
            MimeType = mimeType;
        }
    }
}
