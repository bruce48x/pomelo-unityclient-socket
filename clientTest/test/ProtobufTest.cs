using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Pomelo.Protobuf;

namespace Pomelo.DotNetClient.Test
{
    public class ProtobufTest
    {
        public static JObject read(string name)
        {
            StreamReader file = new StreamReader(name);

            String str = file.ReadToEnd();
            return JObject.Parse(str);
        }
    }
}