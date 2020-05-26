using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace Algorand
{
    public class Logic
    {
        private static int MAX_COST = 20000;
        private static int MAX_LENGTH = 1000;
        private static int INTCBLOCK_OPCODE = 32;
        private static int BYTECBLOCK_OPCODE = 38;
        private static LangSpec langSpec;
        private static Operation[] opcodes;

        public Logic()
        {
        }

        public static bool CheckProgram(byte[] program, List<byte[]> args)
        {
            if (langSpec == null)
            {
              
                //     var jsonStr = (new StreamReader("langspec.json")).ReadToEnd();
                //     langSpec = JsonConvert.DeserializeObject<LangSpec>(jsonStr);
                
                // read file from embedded resources - not the file system
                var jsonStr = GetFromResources("langspec.json");
                langSpec = JsonConvert.DeserializeObject<LangSpec>(jsonStr);

                //                InputStreamReader reader;
                //                try {
                //                reader = new InputStreamReader(Logic.class.getResourceAsStream("/langspec.json"), "UTF-8");
                //            } catch (UnsupportedEncodingException var11) {
                //                throw new IllegalStateException("langspec opening error");
                // }

                // Gson g = (new GsonBuilder()).create();
                //        langSpec = (Logic.LangSpec) g.fromJson(reader, Logic.LangSpec.class);
                //            reader.close();
            }

            VarintResult result = Uvarint.parse(program);
            int vlen = result.length;
            if (vlen <= 0)
            {
                throw new ArgumentException("version parsing error");
            }
            else
            {
                int version = result.value;
                if (version > langSpec.EvalMaxVersion)
                {
                    throw new ArgumentException("unsupported version");
                }
                else
                {
                    if (args == null)
                    {
                        args = new List<byte[]>();
                    }

                    int cost = 0;
                    int length = program.Length;

                    int pc;
                    for (pc = 0; pc < args.Count; ++pc)
                    {
                        length += args[pc].Length;
                    }

                    if (length > 1000)
                    {
                        throw new ArgumentException("program too long");
                    }
                    else
                    {
                        if (opcodes == null)
                        {
                            opcodes = new Logic.Operation[256];

                            for (pc = 0; pc < langSpec.Ops.Length; ++pc)
                            {
                                Logic.Operation op = langSpec.Ops[pc];
                                opcodes[op.Opcode] = op;
                            }
                        }

                        int size;
                        for (pc = vlen; pc < program.Length; pc += size)
                        {
                            int opcode = program[pc] & 255;
                            Logic.Operation op = opcodes[opcode];
                            if (op == null)
                            {
                                throw new ArgumentException("invalid instruction");
                            }

                            cost += op.Cost;
                            size = op.Size;
                            if (size == 0)
                            {
                                switch (op.Opcode)
                                {
                                    case 32:
                                        size = CheckIntConstBlock(program, pc);
                                        break;
                                    case 38:
                                        size = CheckByteConstBlock(program, pc);
                                        break;
                                    default:
                                        throw new ArgumentException("invalid instruction");
                                }
                            }
                        }

                        if (cost > 20000)
                        {
                            throw new ArgumentException("program too costly to run");
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
        }
        internal static string GetFromResources(string resourceName)
        {
            Assembly assem = Assembly.GetExecutingAssembly();

            using (Stream stream = assem.GetManifestResourceStream(assem.GetName().Name + '.' + resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        static int CheckIntConstBlock(byte[] program, int pc)
        {
            int size = 1;
            VarintResult result = Uvarint.parse(JavaHelper<byte>.ArrayCopyRange(program, pc + size, program.Length));
            if (result.length <= 0)
            {
                throw new ArgumentException(string.Format("could not decode int const block at pc=%d", pc));
            }
            else
            {
                size = size + result.length;
                int numInts = result.value;

                for (int i = 0; i < numInts; ++i)
                {
                    if (pc + size >= program.Length)
                    {
                        throw new ArgumentException("int const block exceeds program length");
                    }
                    result = Uvarint.parse(JavaHelper<byte>.ArrayCopyRange(program, pc + size, program.Length));
                    if (result.length <= 0)
                    {
                        throw new ArgumentException(string.Format("could not decode int const[%d] block at pc=%d", i, pc + size));
                    }

                    size += result.length;
                }
                return size;
            }
        }

        static int CheckByteConstBlock(byte[] program, int pc)
        {
            int size = 1;
            VarintResult result = Uvarint.parse(JavaHelper<byte>.ArrayCopyRange(program, pc + size, program.Length));
            if (result.length <= 0)
            {
                throw new ArgumentException(String.Format("could not decode byte[] const block at pc=%d", pc));
            }
            else
            {
                size = size + result.length;
                int numInts = result.value;

                for (int i = 0; i < numInts; ++i)
                {
                    if (pc + size >= program.Length)
                    {
                        throw new ArgumentException("byte[] const block exceeds program length");
                    }

                    result = Uvarint.parse(JavaHelper<byte>.ArrayCopyRange(program, pc + size, program.Length));
                    if (result.length <= 0)
                    {
                        throw new ArgumentException(String.Format("could not decode byte[] const[%d] block at pc=%d", i, pc + size));
                    }

                    size += result.length;
                    if (pc + size >= program.Length)
                    {
                        throw new ArgumentException("byte[] const block exceeds program length");
                    }

                    size += result.value;
                }

                return size;
            }
        }

        private class Operation
        {
            public int Opcode;
            public String Name;
            public int Cost;
            public int Size;
            public String Returns;
            public String[] ArgEnum;
            public String ArgEnumTypes;
            public String Doc;
            public String ImmediateNote;
            public String[] Group;

            private Operation()
            {
            }
        }

        private class LangSpec
        {
            public int EvalMaxVersion;
            public int LogicSigVersion;
            public Logic.Operation[] Ops;

            private LangSpec()
            {
            }
        }
    }
    class Uvarint
    {
        Uvarint()
        {
        }

        public static VarintResult parse(byte[] data)
        {
            int x = 0;
            int s = 0;

            for (int i = 0; i < data.Length; ++i)
            {
                int b = data[i] & 255;
                if (b < 128)
                {
                    if (i <= 9 && (i != 9 || b <= 1))
                    {
                        return new VarintResult(x | (b & 255) << s, i + 1);
                    }

                    return new VarintResult(0, -(i + 1));
                }

                x |= (b & 127 & 255) << s;
                s += 7;
            }

            return new VarintResult();
        }
    }
    class VarintResult
    {
        public int value;
        public int length;

        public VarintResult(int value, int length)
        {
            this.value = value;
            this.length = length;
        }

        public VarintResult()
        {
            this.value = 0;
            this.length = 0;
        }
    }
}