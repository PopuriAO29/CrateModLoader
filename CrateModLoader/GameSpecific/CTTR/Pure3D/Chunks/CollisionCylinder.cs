﻿using System.IO;
using System.Text;

namespace Pure3D.Chunks
{
    [ChunkType(117506051)]
    public class CollisionCylinder : Chunk
    {
        public byte[] Data;

        public CollisionCylinder(File file, uint type) : base(file, type)
        {

        }

        public override void ReadHeader(Stream stream, long length)
        {
            Data = new BinaryReader(stream).ReadBytes((int)length);
        }

        public override void WriteHeader(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(Data);
        }

        public override string ToString()
        {
            return $"Collision Cylinder";
        }
    }
}