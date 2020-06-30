using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Pomelo.Protobuf.Test;

namespace Pomelo.DotNetClient.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            byte[] bytes = Pomelo.Protobuf.Encoder.encodeUInt32(112321);
            Console.WriteLine(Pomelo.Protobuf.Decoder.decodeUInt32(bytes));
            CodecTest.Run();
            ProtobufTest.Run();
            TransportTest.Run();
            ClientTest.Run();
        }
    }
}
