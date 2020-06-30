using System;
using System.IO;
using Pomelo.Protobuf;

namespace Pomelo.DotNetClient.Test
{
    public class CodecTest
    {
        public static bool EncodeSInt32Test(int count)
        {
            Random random = new Random();

            int flag = -1;
            for (int i = 0; i < count; i++)
            {
                flag *= -1;
                int num = random.Next(0, 0x7fffffff) * flag;
                byte[] bytes = Encoder.encodeSInt32(num);
                int result = Decoder.decodeSInt32(bytes);
                if (num != result) return false;
            }

            return true;
        }

        public static bool EncodeUInt32Test(int count)
        {
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                uint num = (uint)random.Next(0, 0x7fffffff);
                byte[] bytes = Encoder.encodeUInt32(num);
                uint result = Decoder.decodeUInt32(bytes);
                if (num != result) return false;
            }

            return true;
        }
    }
}