using System;

namespace Bread2Unity
{
    public class ByteBuffer
    {
        private readonly byte[] _bytes;

        public ByteBuffer(byte[] bytes)
        {
            _bytes = bytes;
            ReadPoint = 0;
        }

        public int ReadPoint { get; private set; }

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
            var result = BitConverter.ToSingle(_bytes, ReadPoint);
            ReadPoint += sizeof(float);
            return result;
        }

        public byte ReadByte()
        {
            var result = _bytes[ReadPoint];
            ReadPoint += sizeof(byte);
            return result;
        }

        public int ReadInt()
        {
            var result = BitConverter.ToInt32(_bytes, ReadPoint);
            ReadPoint += sizeof(int);
            return result;
        }
    }
}