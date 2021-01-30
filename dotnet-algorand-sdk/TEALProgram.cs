using Newtonsoft.Json;
using System;

namespace Algorand
{
    /// <summary>
    /// TEALProgram
    /// </summary>
    [JsonConverter(typeof(BytesConverter))]
    public class TEALProgram
    {
        private byte[] program = null;
        public byte[] Bytes
        {
            get
            {
                if (program == null) return null;
                return JavaHelper<byte>.ArrayCopyOf(program, program.Length);
            }
        }

        /// <summary>
        /// default values for serializer to ignore
        /// </summary>
        public TEALProgram() { }

        /// <summary>
        /// Initialize a TEALProgram based on the byte array.An ArgumentException is thrown if the program is invalid.
        /// </summary>
        /// <param name="program">program</param>
        [JsonConstructor]
        public TEALProgram(byte[] program)
        {
            if (program == null) return;
            try
            {
                Logic.ReadProgram(program, null);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
            this.program = JavaHelper<byte>.ArrayCopyOf(program, program.Length);
        }

        /// <summary>
        /// Initialize a TEALProgram based on the base64 encoding.
        /// A runtime exception is thrown if the program is invalid.
        /// </summary>
        /// <param name="base64String">base64String</param>
        public TEALProgram(string base64String) : this(Convert.FromBase64String(base64String)) { }
    }
}