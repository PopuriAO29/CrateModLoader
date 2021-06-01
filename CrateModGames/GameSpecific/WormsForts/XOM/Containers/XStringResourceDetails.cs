﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CrateModLoader.GameSpecific.WormsForts.XOM
{
    [XOM_TypeName("XStringResourceDetails")]
    public class XStringResourceDetails : Container
    {
        public VInt NameKey = new VInt();
        public VInt ValueKey = new VInt();
        public uint Flags;

        public override void Read(BinaryReader reader)
        {
            ValueKey = new VInt(reader);
            NameKey = new VInt(reader);
            Flags = reader.ReadUInt32();

            if (ParentFile.Strings.ContainsKey(NameKey.RawValue) && ParentFile.Strings.ContainsKey(ValueKey.RawValue))
            {
                //Console.WriteLine("XString: " + ParentFile.Strings[NameKey.RawValue] + " - " + ParentFile.Strings[ValueKey.RawValue]);
            }
            else
            {
                Console.WriteLine("XString: KEY NOT FOUND: " + NameKey.RawValue + " / " + ValueKey.RawValue);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            ValueKey.Write(writer);
            NameKey.Write(writer);
            writer.Write(Flags);
        }
    }
}
