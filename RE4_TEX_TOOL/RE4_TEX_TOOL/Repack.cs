using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SimpleEndianBinaryIO;
using System.Globalization;

namespace RE4_TEX_TOOL
{
    internal static class Repack
    {
        public static void RepackFile(FileInfo fileInfo, IsVersion isVersion)
        {
            string baseDirectory = Path.GetDirectoryName(fileInfo.FullName);
            string baseFileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);

            Endianness endianness = Endianness.LittleEndian;
            if (isVersion == IsVersion.GCWII || isVersion == IsVersion.X360)
            {
                endianness = Endianness.BigEndian;
            }

            TexData[] texDatas = LoadIdx(fileInfo);

            uint iCount = 0; // amount of TPL
            bool asFile = true;

            while (asFile)
            {
                string tplpath = Path.Combine(baseDirectory, baseFileName + "_TEX_" + iCount.ToString("D") + ".TPL");

                if (File.Exists(tplpath))
                {
                    iCount++;
                }
                else
                {
                    asFile = false;
                }
            }

            FileContent[] TPLs = new FileContent[iCount];

            for (int i = 0; i < iCount; i++)
            {
                string tplpath = Path.Combine(baseDirectory, baseFileName + "_TEX_" + i.ToString("D") + ".TPL");
                TPLs[i].Arr = File.ReadAllBytes(tplpath);
            }

            // make file

            var bw = new EndianBinaryWriter(new FileInfo(Path.Combine(baseDirectory, baseFileName + ".TEX")).Create(), endianness);
            bw.Write((uint)0x3);
            bw.Write((uint)0x20);
            bw.Write(new byte[0x18]);

            //table1
            bw.Write((uint)texDatas.Length);
            for (int i = 0; i < texDatas.Length; i++)
            {
                bw.Write((ushort)texDatas[i].RefID);
                bw.Write(new byte[0x6]);// padding
            }

            //alignment
            long diff = bw.Position % 32;
            if (diff != 0)
               { bw.Write(new byte[32 - diff]); }

            if (texDatas.Length != 0 && TPLs.Length != 0)
            {

                //tpl
                uint offsetToSet = (uint)bw.Position;
                bw.Position = 0x8;
                bw.Write((uint)offsetToSet);
                bw.Position = offsetToSet;

                bw.Write((uint)TPLs.Length);
                long offsetToOffsetTpl = bw.Position;
                bw.Write(new byte[TPLs.Length * 4]); // offsets

                //alignment
                diff = bw.Position % 32;
                if (diff != 0)
                   { bw.Write(new byte[32 - diff]); }

                uint offsetToSetTpl = (uint)bw.Position;

                for (int i = 0; i < TPLs.Length; i++)
                {
                    bw.Position = offsetToOffsetTpl;
                    bw.Write((uint)(offsetToSetTpl - offsetToSet));
                    bw.Position = offsetToSetTpl;

                    bw.Write(TPLs[i].Arr);
                    diff = bw.Position % 16;
                    if (diff != 0)
                       { bw.Write(new byte[16 - diff]);}

                    offsetToSetTpl = (uint)bw.Position;
                    offsetToOffsetTpl += 4;
                }

                //alignment
                diff = bw.Position % 32;
                if (diff != 0)
                   { bw.Write(new byte[32 - diff]); }

                //DATA
                offsetToSet = (uint)bw.Position;
                bw.Position = 0xC;
                bw.Write((uint)offsetToSet);
                bw.Position = offsetToSet;

                bw.Write((uint)texDatas.Length);
                long offsetToOffsetDATA = bw.Position;
                bw.Write(new byte[texDatas.Length * 4]); // offsets

                //alignment
                diff = bw.Position % 32;
                if (diff != 0)
                   { bw.Write(new byte[32 - diff]); }

                uint offsetToSetDATA = (uint)bw.Position;

                for (int i = 0; i < texDatas.Length; i++)
                {
                    bw.Position = offsetToOffsetDATA;
                    bw.Write((uint)(offsetToSetDATA - offsetToSet));
                    bw.Position = offsetToSetDATA;

                    bw.Write((ushort)texDatas[i].Width);
                    bw.Write((ushort)texDatas[i].Height);
                    bw.Write((ushort)texDatas[i].Offset4);
                    bw.Write((ushort)texDatas[i].Offset6);
                    bw.Write((ushort)texDatas[i].Offset8);
                    bw.Write((byte)texDatas[i].Offset10);
                    bw.Write((byte)texDatas[i].Offset11);

                    //alignment
                    diff = bw.Position % 32;
                    bw.Write(new byte[32 - diff]);

                    offsetToSetDATA = (uint)bw.Position;
                    offsetToOffsetDATA += 4;
                }
            }

            bw.Close();
        }


        private static TexData[] LoadIdx(FileInfo fileInfo) 
        {
            Box<TexData> temp = new Box<TexData>( new TexData());
            Dictionary<int, Box<TexData>> data = new Dictionary<int, Box<TexData>>();
            int count = 0;

            var idx = fileInfo.OpenText();

            while (!idx.EndOfStream)
            {
                string line = idx.ReadLine().Trim().ToUpperInvariant();

                if (line == null || line.Length == 0 || line.StartsWith("\\") || line.StartsWith("/") || line.StartsWith("#") || line.StartsWith(":"))
                {
                    continue;
                }
                else if (line.StartsWith("ENTRY"))
                {
                    temp = new Box<TexData>(new TexData());
                    var split = line.Split('_');
                    if (split.Length >= 2)
                    {
                        int entryID = -1;
                        try
                        {
                            entryID = int.Parse(ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }

                        if (entryID > -1 && !data.ContainsKey(entryID))
                        {
                            data.Add(entryID, temp);

                            if (count <= entryID)
                            {
                                count = entryID + 1;
                            }
                        }
                    }
                }

                else
                {
                    _ = SetUshortDec(ref line, "REFID", ref temp.Value.RefID)
                        || SetUshortDec(ref line, "WIDTH", ref temp.Value.Width)
                        || SetUshortDec(ref line, "HEIGHT", ref temp.Value.Height)
                        || SetUshortDec(ref line, "OFFSET4", ref temp.Value.Offset4)
                        || SetUshortDec(ref line, "OFFSET6", ref temp.Value.Offset6)
                        || SetUshortDec(ref line, "OFFSET8", ref temp.Value.Offset8)
                        || SetByteDec(ref line, "OFFSET10", ref temp.Value.Offset10)
                        || SetByteDec(ref line, "OFFSET11", ref temp.Value.Offset11)
                        ;
                }

            }

            TexData[] res = new TexData[count];
            for (int i = 0; i < count; i++)
            {
                if (data.ContainsKey(i))
                {
                    res[i] = data[i].Value;
                }
                else 
                {
                    res[i] = new TexData();
                }
            }
            return res;
        }

        private static string ReturnValidDecValue(string cont)
        {
            string res = "";
            foreach (var c in cont)
            {
                if (char.IsDigit(c))
                {
                    res += c;
                }
            }
            if (res.Length == 0)
            {
                res = "0";
            }
            return res;
        }

        private static bool SetByteDec(ref string line, string key, ref byte varToSet)
        {
            if (line.StartsWith(key))
            {
                var split = line.Split(':');
                if (split.Length >= 2)
                {
                    try
                    {
                        varToSet = byte.Parse(ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                    }
                }
                return true;
            }
            return false;
        }

        private static bool SetUshortDec(ref string line, string key, ref ushort varToSet)
        {
            if (line.StartsWith(key))
            {
                var split = line.Split(':');
                if (split.Length >= 2)
                {
                    try
                    {
                        varToSet = ushort.Parse(ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                    }
                }
                return true;
            }
            return false;
        }
    }
}
