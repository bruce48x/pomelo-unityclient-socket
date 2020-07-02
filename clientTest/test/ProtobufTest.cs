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

        public static bool equal(JObject a, JObject b)
        {
            var aDict = a.ToObject<Dictionary<string, object>>();

            foreach (KeyValuePair<string, object> pair in aDict)
            {
                var key = pair.Key;
                if (a[key].GetType().ToString() == "Newtonsoft.Json.Linq.JObject")
                {
                    if (!equal((JObject)a[key], (JObject)b[key])) return false;
                }
                else if (a[key].GetType().ToString() == "Newtonsoft.Json.Linq.JsonArray")
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