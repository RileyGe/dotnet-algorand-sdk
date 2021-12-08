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
            byte[] program = { 0x01, 0x20, 0x01, 0x01, (byte)0xFF };
            List<byte[]> args = new List<byte[]>();

            var ex = Assert.Throws<ArgumentException>(() => { Logic.CheckProgram(program, args); });
            Assert.AreEqual("invalid instruction: 255", ex.Message);
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

        [Test]
        public void testCheckProgramTealV3()
        {
            Assert.GreaterOrEqual(Logic.GetEvalMaxVersion(), 3);
            Assert.GreaterOrEqual(Logic.GetLogicSigVersion(), 3);

            // min_balance
            {

                byte[] program = {
                0x03, 0x20, 0x01, 0x00, 0x22, 0x78  // int 0; min_balance
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
            // pushbytes
            {
                byte[] program = {
                    0x03, 0x20, 0x01, 0x00, 0x22, (byte)0x80, 0x02, 0x68, 0x69, 0x48  // int 0; pushbytes "hi"; pop
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
            // pushint
            {
                byte[] program = {
                    0x03, 0x20, 0x01, 0x00, 0x22, (byte)0x81, 0x01, 0x48  // int 0; pushint 1; pop
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }

            // swap
            {
                byte[] program = {
                    0x03, 0x20, 0x02, 0x00, 0x01, 0x22, 0x23, 0x4c, 0x48  // int 0; int 1; swap; pop
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
        }

        [Test]
        public void testCheckProgramTealV4()
        {
            Assert.GreaterOrEqual(Logic.GetEvalMaxVersion(), 4);

            // divmodw
            {
                byte[] program = {
                    0x04, 0x20, 0x03, 0x01, 0x00, 0x02, 0x22,  0x81,  0xd0,  0x0f, 0x23, 0x24, 0x1f  // int 1; pushint 2000; int 0; int 2; divmodw
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
            // gloads i
            {
                byte[] program = {
                    0x04, 0x20, 0x01, 0x00, 0x22, 0x3b, 0x00  // int 0; gloads 0
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
            // callsub
            {
                byte[] program = {
                    0x04, 0x20, 0x02, 0x01, 0x02, 0x22,  0x88, 0x00, 0x02, 0x23, 0x12, 0x49  // int 1; callsub double; int 2; ==; double: dup;
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
            // b>=
            {
                byte[] program = {
                    0x04, 0x26, 0x02, 0x01, 0x11, 0x01, 0x10, 0x28, 0x29,  0xa7  // byte 0x11; byte 0x10; b>=
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
            // b^
            {
                byte[] program = {
                    0x04, 0x26, 0x02, 0x01, 0x11, 0x01, 0x10, 0x28, 0x29,  0xa7  // byte 0x11; byte 0x10; b^; byte 0x01; ==
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
            // callsub, retsub.
            {
                byte[] program = {
                    0x04, 0x20, 0x02, 0x01, 0x02, 0x22,  0x88, 0x00, 0x03, 0x23, 0x12, 0x43, 0x49, 0x08,  0x89  // int 1; callsub double; int 2; ==; return; double: dup; +; retsub;
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
            // loop
            {
                byte[] program = {
                    0x04, 0x20, 0x04, 0x01, 0x02, 0x0a, 0x10, 0x22, 0x23, 0x0b, 0x49, 0x24, 0x0c, 0x40,  0xff,  0xf8, 0x25, 0x12  // int 1; loop: int 2; *; dup; int 10; <; bnz loop; int 16; ==
                };
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
        }

        [Test]
        public void testCheckProgramTealV5()
        {
            Assert.GreaterOrEqual(Logic.GetEvalMaxVersion(), 5);
            
            // itxn ops
            {
                byte[] program = {
                    0x05, 0x20, 0x01,  0xc0,  0x84, 0x3d,  0xb1,  0x81, 0x01,  0xb2, 0x10, 0x22,  0xb2, 0x08, 0x31, 0x00,  0xb2, 0x07,  0xb3,  0xb4, 0x08, 0x22, 0x12
                };
                // itxn_begin; int pay; itxn_field TypeEnum; int 1000000; itxn_field Amount; txn Sender; itxn_field Receiver; itxn_submit; itxn Amount; int 1000000; ==
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }

            // ECDSA ops
            {
                byte[] program = {
                    0x05,0x80,0x08,0x74,0x65,0x73,0x74,0x64,0x61,0x74,0x61,0x03,0x80,0x20,0x79,0xbf,0xa8,0x24,0x5a,0xea,0xc0,0xe7,0x14,0xb7,0xbd,0x2b,0x32,0x52,0xd0,0x39,0x79,0xe5,0xe7,0xa4,0x3c,0xb0,0x39,0x71,0x5a,0x5f,0x81,0x09,0xa7,0xdd,0x9b,0xa1,0x80,0x20,0x07,0x53,0xd3,0x17,0xe5,0x43,0x50,0xd1,0xd1,0x02,0x28,0x9a,0xfb,0xde,0x30,0x02,0xad,0xd4,0x52,0x9f,0x10,0xb9,0xf7,0xd3,0xd2,0x23,0x84,0x39,0x85,0xde,0x62,0xe0,0x80,0x21,0x03,0xab,0xfb,0x5e,0x6e,0x33,0x1f,0xb8,0x71,0xe4,0x23,0xf3,0x54,0xe2,0xbd,0x78,0xa3,0x84,0xef,0x7c,0xb0,0x7a,0xc8,0xbb,0xf2,0x7d,0x2d,0xd1,0xec,0xa0,0x0e,0x73,0xc1,0x06,0x00,0x05,0x00
                };
                // byte "testdata"; sha512_256; byte 0x79bfa8245aeac0e714b7bd2b3252d03979e5e7a43cb039715a5f8109a7dd9ba1; byte 0x0753d317e54350d1d102289afbde3002add4529f10b9f7d3d223843985de62e0; byte 0x03abfb5e6e331fb871e423f354e2bd78a384ef7cb07ac8bbf27d2dd1eca00e73c1; ecdsa_pk_decompress Secp256k1; ecdsa_verify Secp256k1
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }

            // cover, uncover, log
            {
                byte[] program = {
                    0x05,  0x80, 0x01, 0x61,  0x80, 0x01, 0x62,  0x80, 0x01, 0x63, 0x4e, 0x02, 0x4f, 0x02, 0x50, 0x50,  0xb0,  0x81, 0x01
                };
                // byte "a"; byte "b"; byte "c"; cover 2; uncover 2; concat; concat; log; int 1
                bool valid = Logic.CheckProgram(program, null);
                Assert.IsTrue(valid);
            }
        }

    }
}
