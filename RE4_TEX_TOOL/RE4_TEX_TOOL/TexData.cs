using System;
using System.Collections.Generic;
using System.Text;

namespace RE4_TEX_TOOL
{
    internal struct TexData
    {
        public ushort RefID;
        public ushort Width;
        public ushort Height;
        public ushort Offset4;
        public ushort Offset6;
        public ushort Offset8;
        public byte Offset10;
        public byte Offset11;
    }

    internal struct FileContent
    {
        public byte[] Arr;
    }

    internal class Box<T>
    {
        public T Value;

        public Box(T value)
        {
            Value = value;
        }
    }
}
