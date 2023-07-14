using System.Collections.Generic;
using AssetsTools.NET.Atomic.Helper;

namespace AssetsTools.NET
{
    public class ClassDatabaseTypeNode
    {
        public volatile ushort TypeName;
        public volatile ushort FieldName;
        public volatile int ByteSize;
        public volatile ushort Version;
        public volatile byte TypeFlags;
        public volatile uint MetaFlag;
        public ConcurrentList<ClassDatabaseTypeNode> Children { get; set; }

        /// <summary>
        /// Read the <see cref="ClassDatabaseTypeNode"/> with the provided reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public void Read(AssetsFileReader reader)
        {
            TypeName = reader.ReadUInt16();
            FieldName = reader.ReadUInt16();
            ByteSize = reader.ReadInt32();
            Version = reader.ReadUInt16();
            TypeFlags = reader.ReadByte();
            MetaFlag = reader.ReadUInt32();

            int childrenCount = reader.ReadUInt16();
            Children = new ConcurrentList<ClassDatabaseTypeNode>(childrenCount);
            for (int i = 0; i < childrenCount; i++)
            {
                ClassDatabaseTypeNode child = new ClassDatabaseTypeNode();
                child.Read(reader);
                Children.Add(child);
            }
        }
    }
}
