using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    public record DbEncryptionSettings(byte[] Key, byte[] Iv);
}
