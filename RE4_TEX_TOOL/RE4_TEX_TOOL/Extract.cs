using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SimpleEndianBinaryIO;

namespace RE4_TEX_TOOL
{
    internal static class Extract
    {
        public static void ExtractFile(FileInfo fileInfo, IsVersion isVersion) 
        {
            string baseDirectory = Path.GetDirectoryName(fileInfo.FullName);
            string baseFileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);

            Endianness endianness = Endianness.LittleEndian;
            if (isVersion == IsVersion.GCWII || isVersion == IsVersion.X360)
            {
                endianness = Endianness.BigEndian;
            }

            var tex = new EndianBinaryReader(fileInfo.OpenRead(), endianness);
            uint Amount = tex.ReadUInt32();

            if (Amount != 3)
            {
                Console.WriteLine("Invalid File!");
                tex.Close();
                return;
            }

            uint Addr1 = tex.ReadUInt32(); // ID
            uint Addr2 = tex.ReadUInt32(); // TPL
            uint Addr3 = tex.ReadUInt32(); // DATA

            if (Addr2 == 0 || Addr3 == 0)
            {
                Console.WriteLine("TEX is empty!");
            }

            Dictionary<int, TexData> Data = new Dictionary<int, TexData>();

            if (tex.Length > Addr1 && Addr1 != 0)
            {
                tex.Position = Addr1;
                uint count = tex.ReadUInt32();

                for (int i = 0; i < count; i++)
                {
                    ushort ID = tex.ReadUInt16();
                    tex.ReadBytes(6);//padding
                    Data.Add(i, new TexData() {RefID = ID });
                }
            }

            if (Addr3 != 0)
            {
                tex.Position = Addr3;
                uint count = tex.ReadUInt32();
                uint[] offsets = new uint[count];
                for (int i = 0; i < count; i++)
                {
                    offsets[i] = tex.ReadUInt32();
                }

                for (int i = 0; i < count; i++)
                {
                    tex.Position = Addr3 + offsets[i];

                    if (!Data.ContainsKey(i))
                    {
                        Data.Add(i, new TexData());
                    }

                    TexData data = Data[i];

                    data.Width = tex.ReadUInt16();
                    data.Height = tex.ReadUInt16();
                    data.Offset4 = tex.ReadUInt16();
                    data.Offset6 = tex.ReadUInt16();
                    data.Offset8 = tex.ReadUInt16();
                    data.Offset10 = tex.ReadByte();
                    data.Offset11 = tex.ReadByte();
                    Data[i] = data;
                }

            }

            // idxtex
            var idx = new FileInfo(Path.Combine(baseDirectory, baseFileName + ".idxtex")).CreateText();
            idx.WriteLine("# RE4_TEX_TOOL");
            idx.WriteLine("# by: JADERLINK");
            idx.WriteLine("# youtube.com/@JADERLINK");
            idx.WriteLine("# github.com/JADERLINK");
            idx.WriteLine("");
            foreach (var item in Data)
            {
                idx.WriteLine("Entry_" + item.Key);
                idx.WriteLine("RefID:" + item.Value.RefID);
                idx.WriteLine("Width:" + item.Value.Width);
                idx.WriteLine("Height:" + item.Value.Height);
                idx.WriteLine("Offset4:" + item.Value.Offset4);
                idx.WriteLine("Offset6:" + item.Value.Offset6);
                idx.WriteLine("Offset8:" + item.Value.Offset8);
                idx.WriteLine("Offset10:" + item.Value.Offset10);
                idx.WriteLine("Offset11:" + item.Value.Offset11);
                idx.WriteLine("");
            }
            idx.Close();

            //TPL
            if (Addr2 != 0)
            {
                FileContent[] TPLs = new FileContent[0];

                switch (isVersion)
                {
                    case IsVersion.GCWII:
                        TPLs = GCExtract.Extract_TPL(tex, Addr2, Addr3);
                        break;
                    case IsVersion.X360:
                    case IsVersion.UHD:
                        TPLs = UhdExtract.Extract_TPL(tex, Addr2, false, endianness);
                        break;
                    case IsVersion.PS4NS:
                        TPLs = UhdExtract.Extract_TPL(tex, Addr2, true, endianness);
                        break;
                }

                for (int i = 0; i < TPLs.Length; i++)
                {
                    BinaryWriter bwTPL = new BinaryWriter(File.Create(Path.Combine(baseDirectory, baseFileName + "_TEX_" + i.ToString("D") + ".TPL")));
                    bwTPL.Write(TPLs[i].Arr);
                    bwTPL.Close();
                }

            }
            tex.Close();

        }

    }
}
