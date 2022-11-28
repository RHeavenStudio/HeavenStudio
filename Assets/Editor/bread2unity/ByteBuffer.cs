using System;

namespace Bread2Unity
{
    public class ByteBuffer
    {
        private readonly byte[] _bytes;

        public int ReadPoint{ get; private set; }

        public ByteBuffer(byte[] bytes)
        {
            _bytes = bytes;
            ReadPoint = 0;
        }

        public ushort ReadUShort()
        {
            var result = BitConverter.ToUInt16(_bytes, ReadPoint);
            ReadPoint += sizeof(ushort);
            return result;
        }

        public short ReadShort()
        {
            var result = BitConverter.ToInt16(_bytes, ReadPoint);
            ReadPoint += sizeof(short);
            return result;
        }

        public float ReadFloat()
        {
            float result = BitConverter.ToSingle(_bytes, ReadPoint);
            ReadPoint += sizeof(float);
            return result;
        }

        public byte ReadByte()
        {
            byte result = _bytes[ReadPoint];
            ReadPoint += sizeof(byte);
            return result;
        }

        public int ReadInt()
        {
            int result = BitConverter.ToInt32(_bytes, ReadPoint);
            ReadPoint += sizeof(int);
            return result;
        }
    }
}