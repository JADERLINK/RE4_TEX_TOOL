using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SimpleEndianBinaryIO;

namespace RE4_TEX_TOOL
{
    internal static class GCExtract
    {
        public static FileContent[] Extract_TPL(EndianBinaryReader br, long StartOffset, long EndOffset)
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
                    br.BaseStream.Position = start;
                    long end;
                    if (i + 1 < Amount1)
                    {
                        end = offsetArray[i + 1] + StartOffset;
                    }
                    else
                    {
                        end = EndOffset;
                    }

                    int length = (int)(end - start);
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
    }
}
