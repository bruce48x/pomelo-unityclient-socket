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
        public void TestEncode()
        {
            var uint32 = "112311";
            byte[] bytes = Pomelo.Protobuf.Encoder.encodeUInt32(uint32);
            var decoded = Pomelo.Protobuf.Decoder.decodeUInt32(bytes);
            Assert.AreEqual(Convert.ToUInt32(uint32), decoded);
            Assert.IsTrue(CodecTest.EncodeSInt32Test(10000));
            Assert.IsTrue(CodecTest.EncodeUInt32Test(10000));
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
                Assert.IsNotNull(bytes);
                JsonObject result = protobuf.decode(key, bytes);
                Assert.IsNotNull(result);
                Assert.IsTrue(ProtobufTest.equal(msg, result));
            }
        }
    }
}
