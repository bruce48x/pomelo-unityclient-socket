using System;
using System.Text;
using SimpleJson;
using System.Collections;
using System.Collections.Generic;

namespace Pomelo.Protobuf
{
    public class MsgDecoder
    {
        private JsonObject protos { set; get; }//The message format(like .proto file)
        private int offset { set; get; }
        private byte[] buffer { set; get; }//The binary message from server.
        private Util util { set; get; }

        public MsgDecoder(JsonObject protos)
        {
            if (protos == null) protos = new JsonObject();

            this.protos = (JsonObject)protos["nested"];
            util = new Util();
        }

        /// <summary>
        /// Decode message from server.
        /// </summary>
        /// <param name='route'>
        /// Route.
        /// </param>
        /// <param name='buf'>
        /// JsonObject.
        /// </param>
        public JsonObject decode(string route, byte[] buf)
        {
            buffer = buf;
            offset = 0;
            JsonObject proto = util.GetProtoMessage(protos, route);
            if (!(proto is null))
            {
                JsonObject msg = new JsonObject();
                return decodeMsg(msg, proto, buffer.Length);
            }
            return null;
        }


        /// <summary>
        /// Decode the message.
        /// </summary>
        /// <returns>
        /// The message.
        /// </returns>
        /// <param name='msg'>
        /// JsonObject.
        /// </param>
        /// <param name='proto'>
        /// JsonObject.
        /// </param>
        /// <param name='length'>
        /// int.
        /// </param>
        private JsonObject decodeMsg(JsonObject msg, JsonObject proto, int length)
        {
            while (offset < length)
            {
                Dictionary<string, int> head = getHead();
                int id;
                if (head.TryGetValue("id", out id))
                {
                    object fields = null;
                    if (proto.TryGetValue("fields", out fields))
                    {
                        ICollection<string> keys = ((JsonObject)fields).Keys;
                        foreach (string name in keys)
                        {
                            object field;
                            if (((JsonObject)fields).TryGetValue(name, out field))
                            {
                                object fieldId;
                                if (((JsonObject)field).TryGetValue("id", out fieldId))
                                {
                                    if (Convert.ToInt32(fieldId) == id)
                                    {
                                        object type;
                                        if (((JsonObject)field).TryGetValue("type", out type))
                                        {
                                            object rule;
                                            if (((JsonObject)field).TryGetValue("rule", out rule))
                                            {
                                                if (rule.ToString() == "repeated")
                                                {
                                                    object msgVal;
                                                    if (msg.TryGetValue(name.ToString(), out msgVal))
                                                    {
                                                        decodeArray((List<object>)msgVal, type.ToString(), proto);
                                                    }
                                                    else
                                                    {
                                                        msg.Add(name.ToString(), new List<object>());
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                msg.Add(name.ToString(), decodeProp(type.ToString(), proto));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return msg;
        }

        /// <summary>
        /// Decode array in message.
        /// </summary>
        private void decodeArray(List<object> list, string type, JsonObject proto)
        {
            if (util.isSimpleType(type))
            {
                int length = (int)Decoder.decodeUInt32(getBytes());
                for (int i = 0; i < length; i++)
                {
                    list.Add(decodeProp(type, null));
                }
            }
            else
            {
                list.Add(decodeProp(type, proto));
            }
        }

        /// <summary>
        /// Decode each simple type in message.
        /// </summary>
        private object decodeProp(string type, JsonObject proto)
        {
            switch (type)
            {
                case "uint32":
                    return Decoder.decodeUInt32(getBytes());
                case "int32":
                case "sint32":
                    return Decoder.decodeSInt32(getBytes());
                case "uint64":
                    return Decoder.decodeUInt64(getBytes());
                case "int64":
                case "sint64":
                    return Decoder.decodeSInt64(getBytes());
                case "float":
                    return decodeFloat();
                case "double":
                    return decodeDouble();
                case "string":
                    return decodeString();
                case "bool":
                    return decodeBool();
                default:
                    return decodeObject(type, proto);
            }
        }

        //Decode the user-defined object type in message.
        private JsonObject decodeObject(string type, JsonObject proto)
        {
            if (proto != null)
            {
                JsonObject subProto = util.GetProtoMessage(proto, type);
                int l = (int)Decoder.decodeUInt32(getBytes());
                JsonObject msg = new JsonObject();
                return decodeMsg(msg, subProto, offset + l);
            }
            return new JsonObject();
        }

        //Decode string type.
        private string decodeString()
        {
            int length = (int)Decoder.decodeUInt32(getBytes());
            string msg_string = Encoding.UTF8.GetString(buffer, offset, length);
            offset += length;
            return msg_string;
        }

        //Decode double type.
        private double decodeDouble()
        {
            double msg_double = BitConverter.Int64BitsToDouble((long)ReadRawLittleEndian64());
            offset += 8;
            return msg_double;
        }

        //Decode float type
        private float decodeFloat()
        {
            float msg_float = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            return msg_float;
        }

        private bool decodeBool()
        {
            bool res = Convert.ToBoolean(buffer[offset]);
            offset++;
            return res;
        }

        //Read long in littleEndian
        private ulong ReadRawLittleEndian64()
        {
            ulong b1 = buffer[offset];
            ulong b2 = buffer[offset + 1];
            ulong b3 = buffer[offset + 2];
            ulong b4 = buffer[offset + 3];
            ulong b5 = buffer[offset + 4];
            ulong b6 = buffer[offset + 5];
            ulong b7 = buffer[offset + 6];
            ulong b8 = buffer[offset + 7];
            return b1 | (b2 << 8) | (b3 << 16) | (b4 << 24)
                  | (b5 << 32) | (b6 << 40) | (b7 << 48) | (b8 << 56);
        }

        //Get the type and tag.
        private Dictionary<string, int> getHead()
        {
            int tag = (int)Decoder.decodeUInt32(getBytes());
            Dictionary<string, int> head = new Dictionary<string, int>();
            head.Add("type", tag & 0x7);
            head.Add("id", tag >> 3);
            return head;
        }

        //Get bytes.
        private byte[] getBytes()
        {
            List<byte> arrayList = new List<byte>();
            int pos = offset;
            byte b;
            do
            {
                b = buffer[pos];
                arrayList.Add(b);
                pos++;
            } while (b >= 128);
            offset = pos;
            int length = arrayList.Count;
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = arrayList[i];
            }
            return bytes;
        }
    }
}