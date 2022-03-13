using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Offline_Support
{
    public class SignatureTemplate
    {
        // holds signature in this type "8B CF 8B D0 E8 ?? ?? ?? ?? 85 C0 74 19 A1"
        public string signature = "";

        // offset is added to the final address returned by function below
        public int offset = 0;

        // template for better display, clarity and management in signature scanning loop
        public SignatureTemplate(string signature, int offset)
        {
            this.signature = signature;
            this.offset = offset;
        }
    }
}
