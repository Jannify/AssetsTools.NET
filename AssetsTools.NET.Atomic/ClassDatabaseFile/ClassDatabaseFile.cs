using AssetsTools.NET.Extra.Decompressors.LZ4;
using LZ4ps;
using SevenZip.Compression.LZMA;
using System;
using System.IO;
using AssetsTools.NET.Atomic.Helper;

namespace AssetsTools.NET.Atomic
{
    public class ClassDatabaseFile
    {
        public ClassDatabaseFileHeader Header { get; set; }
        public ConcurrentList<ClassDatabaseType> Classes { get; set; }
        public ClassDatabaseStringTable StringTable { get; set; }
        public ConcurrentList<ushort> CommonStringBufferIndices { get; set; }

        /// <summary>
        /// Read the <see cref="ClassDatabaseFile"/> with the provided reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public void Read(AssetsFileReader reader)
        {
            Header ??= new ClassDatabaseFileHeader();
            Header.Read(reader);

            AssetsFileReader dReader = GetDecompressedReader(reader);
            int classCount = dReader.ReadInt32();
            Classes = new ConcurrentList<ClassDatabaseType>(classCount);
            for (int i = 0; i < classCount; i++)
            {
                ClassDatabaseType type = new ClassDatabaseType();
                type.Read(dReader);
                Classes.Add(type);
            }

            StringTable ??= new ClassDatabaseStringTable();
            StringTable.Read(dReader);

            CommonStringBufferIndices ??= new ConcurrentList<ushort>();
            int size = dReader.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                CommonStringBufferIndices.Add(dReader.ReadUInt16());
            }
        }

        private AssetsFileReader GetDecompressedReader(AssetsFileReader reader)
        {
            AssetsFileReader newReader = reader;
            if (Header.CompressionType != ClassFileCompressionType.Uncompressed)
            {
                MemoryStream ms;
                if (Header.CompressionType == ClassFileCompressionType.Lz4)
                {
                    byte[] uncompressedBytes = new byte[Header.DecompressedSize];
                    using (MemoryStream tempMs = new MemoryStream((byte[]) reader.ReadBytes(Header.CompressedSize)))
                    {
                        Lz4DecoderStream decoder = new Lz4DecoderStream(tempMs);
                        decoder.Read(uncompressedBytes, 0, Header.DecompressedSize);
                        decoder.Dispose();
                    }
                    ms = new MemoryStream(uncompressedBytes);
                }
                else if (Header.CompressionType == ClassFileCompressionType.Lzma)
                {
                    using (MemoryStream tempMs = new MemoryStream((byte[]) reader.ReadBytes(Header.CompressedSize)))
                    {
                        ms = SevenZipHelper.StreamDecompress(tempMs);
                    }
                }
                else
                {
                    throw new Exception($"Class database is using invalid compression type {Header.CompressionType}!");
                }

                newReader = new AssetsFileReader(ms);
            }

            return newReader;
        }

        /// <summary>
        /// Find a class database type by type ID.
        /// </summary>
        /// <param name="id">The type's type ID to search for.</param>
        /// <returns>The type of that type ID.</returns>
        public ClassDatabaseType FindAssetClassByID(int id)
        {
            // 5.4-
            if (id < 0)
            {
                id = 0x72;
            }

            foreach (ClassDatabaseType type in Classes)
            {
                if (type.ClassId == id)
                    return type;
            }
            return null;
        }

        /// <summary>
        /// Find a class database type by type name.
        /// </summary>
        /// <param name="name">The type's type name to search for.</param>
        /// <returns>The type of that type name.</returns>
        public ClassDatabaseType FindAssetClassByName(string name)
        {
            foreach (ClassDatabaseType type in Classes)
            {
                if (GetString(type.Name) == name)
                    return type;
            }
            return null;
        }

        // for convenience

        /// <summary>
        /// Get a string from the string table.
        /// </summary>
        /// <param name="index">The index of the string in the table.</param>
        /// <returns>The string at that index.</returns>
        public string GetString(ushort index) => StringTable.GetString(index);
    }
}
