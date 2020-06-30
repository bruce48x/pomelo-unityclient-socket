using System;
using System.Text;
using SimpleJson;
using System.Collections;
using System.Collections.Generic;

namespace Pomelo.Protobuf
{
    public class MsgEncoder
    {
        private JsonObject protos { set; get; }//The message format(like .proto file)
        private Encoder encoder { set; get; }
        private Util util { set; get; }

        public MsgEncoder(JsonObject protos)
        {
            if (protos == null) protos = new JsonObject();

            this.protos = (JsonObject)protos["nested"];
            this.util = new Util();
        }

        /// <summary>
        /// Encode the message from server.
        /// </summary>
        /// <param name='route'>
        /// Route.
        /// </param>
        /// <param name='msg'>
        /// Message.
        /// </param>
        public byte[] encode(string route, JsonObject msg)
        {
            byte[] returnByte = null;
            JsonObject proto = this.util.GetProtoMessage(this.protos,route);
            if (!(proto is null))
            {
                int length = Encoder.byteLength(msg.ToString()) * 2;
                int offset = 0;
                byte[] buff = new byte[length];
                offset = encodeMsg(buff, offset, proto, msg);
                returnByte = new byte[offset];
                for (int i = 0; i < offset; i++)
                {
                    returnByte[i] = buff[i];
                }
            }
            return returnByte;
        }

        /// <summary>
        /// Encode the message.
        /// </summary>
        private int encodeMsg(byte[] buffer, int offset, JsonObject proto, JsonObject msg)
        {
            ICollection<string> msgKeys = msg.Keys;
            foreach (string key in msgKeys)
            {
                JsonObject protoField = this.util.GetField(proto, key);
                if (protoField is null)
                {
                    continue;
                }
                object fieldRule;
                if (protoField.TryGetValue("rule", out fieldRule))
                {
                    if (fieldRule.ToString() == "repeated")
                    {
                        object arr;
                        if (msg.TryGetValue(key, out arr))
                        {
                            if (((List<object>)arr).Count > 0)
                            {
                                offset = encodeArray((List<object>)arr, protoField, offset, buffer, proto);
                            }
                        }
                    }
                }
                else
                {
                    object valueType, valueId;
                    if (protoField.TryGetValue("type", out valueType) && protoField.TryGetValue("id", out valueId))
                    {
                        offset = this.writeBytes(buffer, offset, this.encodeTag(valueType.ToString(), Convert.ToInt32(valueId)));
                        offset = this.encodeProp(msg[key], valueType.ToString(), offset, buffer, proto);
                    }
                }
            }
            return offset;
        }

        /// <summary>
        /// Encode the array type.
        /// </summary>
        private int encodeArray(List<object> msg, JsonObject value, int offset, byte[] buffer, JsonObject proto)
        {
            object valueType, valueId;
            if (value.TryGetValue("type", out valueType) && value.TryGetValue("id", out valueId))
            {
                if (this.util.isSimpleType(valueType.ToString()))
                {
                    offset = this.writeBytes(buffer, offset, this.encodeTag(valueType.ToString(), Convert.ToInt32(valueId)));
                    offset = this.writeBytes(buffer, offset, Encoder.encodeUInt32((uint)msg.Count));
                    foreach (object item in msg)
                    {
                        offset = this.encodeProp(item, valueType.ToString(), offset, buffer, null);
                    }
                }
                else
                {
                    foreach (object item in msg)
                    {
                        offset = this.writeBytes(buffer, offset, this.encodeTag(valueType.ToString(), Convert.ToInt32(valueId)));
                        offset = this.encodeProp(item, valueType.ToString(), offset, buffer, proto);
                    }
                }
            }
            return offset;
        }

        /// <summary>
        /// Encode each item in message.
        /// </summary>
        private int encodeProp(object value, string type, int offset, byte[] buffer, JsonObject proto)
        {
            switch (type)
            {
                case "uint32":
                    this.writeUInt32(buffer, ref offset, value);
                    break;
                case "int32":
                case "sint32":
                    this.writeInt32(buffer, ref offset, value);
                    break;
                case "float":
                    this.writeFloat(buffer, ref offset, value);
                    break;
                case "double":
                    this.writeDouble(buffer, ref offset, value);
                    break;
                case "string":
                    this.writeString(buffer, ref offset, value);
                    break;
                case "bool":
                    this.writeBool(buffer, ref offset, value);
                    break;
                default:
                    JsonObject message = this.util.GetProtoMessage(this.protos, type);
                    if (!(message is null))
                    {
                        byte[] tembuff = new byte[Encoder.byteLength(value.ToString()) * 3];
                        int length = 0;
                        length = this.encodeMsg(tembuff, length, message, (JsonObject)value);
                        offset = writeBytes(buffer, offset, Encoder.encodeUInt32((uint)length));
                        for (int i = 0; i < length; i++)
                        {
                            buffer[offset] = tembuff[i];
                            offset++;
                        }
                    }
                    break;
            }
            return offset;
        }

        //Encode string.
        private void writeString(byte[] buffer, ref int offset, object value)
        {
            int le = Encoding.UTF8.GetByteCount(value.ToString());
            offset = writeBytes(buffer, offset, Encoder.encodeUInt32((uint)le));
            byte[] bytes = Encoding.UTF8.GetBytes(value.ToString());
            this.writeBytes(buffer, offset, bytes);
            offset += le;
        }

        //Encode double.
        private void writeDouble(byte[] buffer, ref int offset, object value)
        {
            WriteRawLittleEndian64(buffer, offset, (ulong)BitConverter.DoubleToInt64Bits(double.Parse(value.ToString())));
            offset += 8;
        }

        //Encode float.
        private void writeFloat(byte[] buffer, ref int offset, object value)
        {
            this.writeBytes(buffer, offset, Encoder.encodeFloat(float.Parse(value.ToString())));
            offset += 4;
        }

        private void writeBool(byte[] buffer, ref int offset, object value)
        {
            offset = writeBytes(buffer, offset, Encoder.encodeBool(value));
        }

        ////Encode UInt32.
        private void writeUInt32(byte[] buffer, ref int offset, object value)
        {
            offset = writeBytes(buffer, offset, Encoder.encodeUInt32(value.ToString()));
        }

        //Encode Int32
        private void writeInt32(byte[] buffer, ref int offset, object value)
        {
            offset = writeBytes(buffer, offset, Encoder.encodeSInt32(value.ToString()));
        }

        //Write bytes to buffer.
        private int writeBytes(byte[] buffer, int offset, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                buffer[offset] = bytes[i];
                offset++;
            }
            return offset;
        }

        //Encode tag.
        private byte[] encodeTag(string type, int tag)
        {
            int flag = this.util.containType(type);
            return Encoder.encodeUInt32((uint)(tag << 3 | flag));
        }


        private void WriteRawLittleEndian64(byte[] buffer, int offset, ulong value)
        {
            buffer[offset++] = ((byte)value);
            buffer[offset++] = ((byte)(value >> 8));
            buffer[offset++] = ((byte)(value >> 16));
            buffer[offset++] = ((byte)(value >> 24));
            buffer[offset++] = ((byte)(value >> 32));
            buffer[offset++] = ((byte)(value >> 40));
            buffer[offset++] = ((byte)(value >> 48));
            buffer[offset++] = ((byte)(value >> 56));
        }
    }
}