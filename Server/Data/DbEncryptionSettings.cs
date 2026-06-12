using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    // Record type to hold the encryption settings for the database
    public record DbEncryptionSettings(byte[] Key, byte[] Iv);
}
