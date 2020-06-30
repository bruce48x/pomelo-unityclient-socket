using System;
using System.Collections.Generic;
using System.IO;
using SimpleJson;
using Pomelo.Protobuf;

namespace Pomelo.DotNetClient.Test
{
    public class ProtobufTest
    {
        public static JsonObject read(string name)
        {
            StreamReader file = new StreamReader(name);

            String str = file.ReadToEnd();

            return (JsonObject)SimpleJson.SimpleJson.DeserializeObject(str);
        }

        public static bool equal(JsonObject a, JsonObject b)
        {
            ICollection<string> keys0 = a.Keys;

            foreach (string key in keys0)
            {
                Console.WriteLine(a[key].GetType());
                if (a[key].GetType().ToString() == "SimpleJson.JsonObject")
                {
                    if (!equal((JsonObject)a[key], (JsonObject)b[key])) return false;
                }
                else if (a[key].GetType().ToString() == "SimpleJson.JsonArray")
                {
                    continue;
                }
                else
                {
                    if (!a[key].ToString().Equals(b[key].ToString())) return false;
                }
            }

            return true;
        }
    }
}