using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SimpleJson;
using System.Collections.Generic;

namespace Pomelo.DotNetClient.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestTransport()
        {
            int num = 10;
            int limit = 1000;

            Transporter tc = new Transporter(null, TransportTest.process);

            List<byte[]> list;

            byte[] buffer = TransportTest.generateBuffers(num, out list);

            int offset = 0;
            while (offset < buffer.Length)
            {
                int length = 200;
                length = (offset + length) > buffer.Length ? buffer.Length - offset : length;

                tc.processBytes(buffer, offset, offset + length);
                offset += length;
            }

            Assert.IsTrue(TransportTest.check(list));
        }

        [TestMethod]
        public void TestZigZag()
        {
            int n;
            byte[] barr;
            uint ui;

            n = -2;
            barr = BitConverter.GetBytes((n << 1) ^ (n >> 31));
            ui = BitConverter.ToUInt32(barr);
            Assert.AreEqual(ui, 3u);

            n = -1;
            barr = BitConverter.GetBytes((n << 1) ^ (n >> 31));
            ui = BitConverter.ToUInt32(barr);
            Assert.AreEqual(ui, 1u);

            n = 0;
            barr = BitConverter.GetBytes((n << 1) ^ (n >> 31));
            ui = BitConverter.ToUInt32(barr);
            Assert.AreEqual(ui, 0u);

            n = 1;
            barr = BitConverter.GetBytes((n << 1) ^ (n >> 31));
            ui = BitConverter.ToUInt32(barr);
            Assert.AreEqual(ui, 2u);

            n = 2;
            barr = BitConverter.GetBytes((n << 1) ^ (n >> 31));
            ui = BitConverter.ToUInt32(barr);
            Assert.AreEqual(ui, 4u);
        }

        [TestMethod]
        public void TestEncode()
        {
            var uint32 = 112311u;
            byte[] bUi32 = Pomelo.Protobuf.Encoder.encodeUInt32(uint32);
            var decoded = Pomelo.Protobuf.Decoder.decodeUInt32(bUi32);
            Assert.AreEqual(uint32, decoded);

            Assert.IsTrue(CodecTest.EncodeSInt32Test(10000));
            Assert.IsTrue(CodecTest.EncodeUInt32Test(10000));

            ulong ui64 = 98765432109;
            var bUi64 = Pomelo.Protobuf.Encoder.encodeUInt64(ui64);
            var decodedUi64 = Pomelo.Protobuf.Decoder.decodeUInt64(bUi64);
            Assert.AreEqual(ui64, decodedUi64);

            long i64 = 98765432109;
            var bI64 = Pomelo.Protobuf.Encoder.encodeSInt64(i64);
            var decodedI64 = Pomelo.Protobuf.Decoder.decodeSInt64(bI64);
            Assert.AreEqual(i64, decodedI64);

            long i64_2 = -3;
            var bI64_2 = Pomelo.Protobuf.Encoder.encodeSInt64(i64_2);
            var decodedI64_2 = Pomelo.Protobuf.Decoder.decodeSInt64(bI64_2);
            Assert.AreEqual(i64_2, decodedI64_2);
        }

        [TestMethod]
        public void TestProtobuf()
        {
            JsonObject protos = ProtobufTest.read("../../../json/rootProtos.json");
            JsonObject msgs = ProtobufTest.read("../../../json/rootMsg.json");

            Pomelo.Protobuf.Protobuf protobuf = new Pomelo.Protobuf.Protobuf(protos, protos);

            ICollection<string> keys = msgs.Keys;

            foreach (string key in keys)
            {
                JsonObject msg = (JsonObject)msgs[key];
                byte[] bytes = protobuf.encode(key, msg);
                Console.WriteLine("bytes" + bytes);
                Assert.IsNotNull(bytes);
                JsonObject result = protobuf.decode(key, bytes);
                Console.WriteLine("result =" + result);
                Assert.IsNotNull(result);
                Assert.IsTrue(ProtobufTest.equal(msg, result));
            }
        }
    }
}
