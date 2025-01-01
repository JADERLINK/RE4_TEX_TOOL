using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SimpleEndianBinaryIO;

namespace RE4_TEX_TOOL
{
    internal static class UhdExtract
    {
        public static FileContent[] Extract_TPL(EndianBinaryReader br, long StartOffset, bool IsPS4NS, Endianness endianness)
        {
            if (StartOffset != 0)
            {
                br.BaseStream.Position = StartOffset;
                uint Amount1 = br.ReadUInt32();

                uint[] offsetArray = new uint[Amount1];

                for (int i = 0; i < Amount1; i++)
                {
                    offsetArray[i] = br.ReadUInt32();
                }
                FileContent[] files = new FileContent[Amount1];

                for (int i = 0; i < Amount1; i++)
                {
                    long start = offsetArray[i] + StartOffset;
                    UhdTPLDecoder(br.BaseStream, start, out long end, IsPS4NS, endianness);
                    int length = (int)(end - start);

                    br.BaseStream.Position = start;
                    FileContent content = new FileContent();
                    content.Arr = br.ReadBytes(length);
                    files[i] = content;
                }
                return files;
            }
            else
            {
                return new FileContent[0];
            }
        }

        private static void UhdTPLDecoder(Stream stream, long startOffset, out long endOffset, bool IsPs4NS, Endianness endianness)
        {
            EndianBinaryReader br = new EndianBinaryReader(stream, endianness);
            br.BaseStream.Position = startOffset;

            uint magic = br.ReadUInt32();
            if (!(magic == 0x78563412 || magic == 0x12345678))
            {
                throw new ArgumentException("Invalid TPL file!");
            }
            uint TplAmount = br.ReadUInt32();
            uint StartOffset = br.ReadUInt32();

            br.BaseStream.Position = StartOffset + startOffset;

            uint[] offsets = new uint[TplAmount];

            for (int i = 0; i < TplAmount; i++)
            {
                uint image_data_offset = br.ReadUInt32();
                if (IsPs4NS)
                {
                    _ = br.ReadUInt32(); // image_data_offset part2
                }

                uint palette_offset = br.ReadUInt32(); // não usado
                if (IsPs4NS)
                {
                    _ = br.ReadUInt32(); // palette_offset part2
                }

                offsets[i] = image_data_offset;
            }

            for (int i = 0; i < TplAmount; i++)
            {
                br.BaseStream.Position = offsets[i] + startOffset;

                ushort width = br.ReadUInt16();
                ushort height = br.ReadUInt16();
                uint PixelFormatType = br.ReadUInt32();
                uint secundOffset = br.ReadUInt32();
                // proximos campos omitidos

                br.BaseStream.Position = secundOffset + startOffset;
                uint PackID = br.ReadUInt32();
                uint TextureID = br.ReadUInt32();
            }

            endOffset = br.BaseStream.Position;
        }

    }
}
