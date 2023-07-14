using AssetsTools.NET.Extra;
using System;
using System.Text;

namespace AssetsTools.NET.Atomic
{
    public class ClassDatabaseFileHeader
    {
        public volatile string Magic;
        public volatile byte FileVersion;
        public UnityVersion Version;
        public volatile ClassFileCompressionType CompressionType;
        public volatile int CompressedSize;
        public volatile int DecompressedSize;

        /// <summary>
        /// Read the <see cref="ClassDatabaseFileHeader"/> with the provided reader.
        /// Note only new CLDB files are supported. Original UABE cldb files are no longer supported.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public void Read(AssetsFileReader reader)
        {
            Magic = reader.ReadStringLength(4);

            if (Magic != "CLDB")
            {
                if (Magic == "cldb")
                    throw new NotSupportedException("Old cldb style class databases are no longer supported.");
                else
                    throw new NotSupportedException("CLDB magic not found. Is this really a class database file?");
            }

            FileVersion = reader.ReadByte();
            if (FileVersion > 1)
                throw new Exception($"Unsupported or invalid file version {FileVersion}.");

            Version = UnityVersion.FromUInt64(reader.ReadUInt64());

            CompressionType = (ClassFileCompressionType)reader.ReadByte();
            CompressedSize = reader.ReadInt32();
            DecompressedSize = reader.ReadInt32();
        }
    }
}
