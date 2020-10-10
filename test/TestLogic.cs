using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Algorand;
using System.Linq;

namespace test
{
    [TestFixture]
    public class TestLogic
    {
        [Test]
        public void testParseUvarint1()
        {
            byte[] data = { 0x01 };
            VarintResult result = Logic.GetUVarint(data, 0);
            Assert.AreEqual(result.length, 1);
            Assert.AreEqual(result.value, 1);
        }

        [Test]
        public void testParseUvarint2()
        {
            byte[]
            data = { 0x02 };
            VarintResult result = Logic.GetUVarint(data, 0);
            Assert.AreEqual(result.length, 1);
            Assert.AreEqual(result.value, 2);
        }

        [Test]
        public void testParseUvarint3()
        {
            byte[]
            data = { 0x7b };
            VarintResult result = Logic.GetUVarint(data, 0);
            Assert.AreEqual(result.length, 1);
            Assert.AreEqual(result.value, 123);
        }

        [Test]
        public void testParseUvarint4()
        {
            byte[]
            data = { (byte)0xc8, 0x03 };
            VarintResult result = Logic.GetUVarint(data, 0);
            Assert.AreEqual(result.length, 2);
            Assert.AreEqual(result.value, 456);
        }

        [Test]
        public void testParseUvarint4AtOffset()
        {
            byte[]
            data = { 0x0, 0x0, (byte)0xc8, 0x03 };
            VarintResult result = Logic.GetUVarint(data, 2);
            Assert.AreEqual(result.length, 2);
            Assert.AreEqual(result.value, 456);
        }

        [Test]
        public void testParseIntcBlock()
        {
            byte[] data = { 0x20, 0x05, 0x00, 0x01, (byte)0xc8, 0x03, 0x7b, 0x02 };

            IntConstBlock results = Logic.ReadIntConstBlock(data, 0);
            Assert.AreEqual(results.size, data.Length);
            TestUtil.ContainsExactlyElementsOf(results.results, new List<int>(new int[] { 0, 1, 456, 123, 2 }));
        }

        [Test]
        public void testParseBytecBlock()
        {
            byte[] data = { 0x026, 0x02, 0x0d, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x02, 0x01, 0x02 };
            List<byte[]> values = new List<byte[]>(){
                    new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33 },
                    new byte[] { 0x1, 0x2 } };

            var results = Logic.ReadByteConstBlock(data, 0);
            Assert.AreEqual(results.size, data.Length);
            CollectionAssert.AreEqual(results.results, values);
            TestUtil.ContainsExactlyElementsOf(results.results, values);
        }

        [Test]
        public void testCheckProgramValid()
        {
            byte[] program = { 0x01, 0x20, 0x01, 0x01, 0x22  // int 1
            };

            // Null argument
            ProgramData programData = Logic.ReadProgram(program, null);
            Assert.IsTrue(programData.good);
            TestUtil.ContainsExactlyElementsOf(programData.intBlock, new List<int>(new int[] { 1 }));
            Assert.IsEmpty(programData.byteBlock);

            // No argument
            List<byte[]> args = new List<byte[]>();
            programData = Logic.ReadProgram(program, args);
            Assert.IsTrue(programData.good);
            TestUtil.ContainsExactlyElementsOf(programData.intBlock, new List<int>(new int[] { 1 }));
            Assert.IsEmpty(programData.byteBlock);

            // Unused argument
            byte[] arg = Enumerable.Repeat((byte)0x31, 10).ToArray();
            args.Add(arg);

            programData = Logic.ReadProgram(program, args);
            Assert.IsTrue(programData.good);
            TestUtil.ContainsExactlyElementsOf(programData.intBlock, new List<int>(new int[] { 1 }));
            Assert.IsEmpty(programData.byteBlock);

            // Repeated int constants parsing
            byte[] int1 = Enumerable.Repeat((byte)0x22, 10).ToArray();
            //byte[] int1 = new byte[10];
            //Arrays.fill(int1, (byte)0x22);
            var program2 = program.ToList();
            program2.AddRange(int1);
            //byte[] program2 = program.ToList().AddRange(int1);

            //JavaHelper<byte>.SyatemArrayCopy(program, 0, program2, 0, program.Length);
            //JavaHelper<byte>.SyatemArrayCopy(int1, 0, program2, program.Length, int1.Length);
            programData = Logic.ReadProgram(program2.ToArray(), args);
            Assert.IsTrue(programData.good);
            TestUtil.ContainsExactlyElementsOf(programData.intBlock, new List<int>(new int[] { 1 }));
            Assert.IsEmpty(programData.byteBlock);
        }

        [Test]
        public void testCheckProgramLongArgs()
        {
            byte[] program = { 0x01, 0x20, 0x01, 0x01, 0x22  // int 1
            };
            List<byte[]> args = new List<byte[]>();
            byte[] arg = Enumerable.Repeat((byte)0x31, 1000).ToArray();
            args.Add(arg);

            var ex = Assert.Throws<ArgumentException>(() => { Logic.ReadProgram(program, args); });
            Assert.AreEqual("program too long", ex.Message);
        }

        [Test]
        public void testCheckProgramLong()
        {
            byte[] program = { 0x01, 0x20, 0x01, 0x01, 0x22  // int 1
        };
            byte[] int1 = new byte[1000];
            var program2 = program.ToList();
            program2.AddRange(int1);
            List<byte[]> args = new List<byte[]>();

            //JavaHelper<byte>.SyatemArrayCopy(program, 0, program2, 0, program.Length);
            //JavaHelper<byte>.SyatemArrayCopy(int1, 0, program2, program.Length, int1.Length);

            var ex = Assert.Throws<ArgumentException>(() => { Logic.CheckProgram(program2.ToArray(), args); });
            Assert.AreEqual("program too long", ex.Message);
        }

        [Test]
        public void testCheckProgramInvalidOpcode()
        {
            byte[] program = { 0x01, 0x20, 0x01, 0x01, (byte)0x81 };
            List<byte[]> args = new List<byte[]>();

            var ex = Assert.Throws<ArgumentException>(() => { Logic.CheckProgram(program, args); });
            Assert.AreEqual("invalid instruction: 129", ex.Message);
        }

        [Test]
        public void testCheckProgramTealV2()
        {
            Assert.GreaterOrEqual(Logic.GetEvalMaxVersion(), 2);
            Assert.GreaterOrEqual(Logic.GetLogicSigVersion(), 2);
            {
                // balance
                byte[] program = { 0x02, 0x20, 0x01, 0x00, 0x22, 0x60  // int 0; balance
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }

            // app_opted_in
            {
                byte[] program = { 0x02, 0x20, 0x01, 0x00, 0x22, 0x22, 0x61  // int 0; int 0; app_opted_in
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }

            {
                // asset_holding_get
                byte[] program = { 0x02, 0x20, 0x01, 0x00, 0x22, 0x70, 0x00  // int 0; int 0; asset_holding_get Balance
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
        }
    }
}
