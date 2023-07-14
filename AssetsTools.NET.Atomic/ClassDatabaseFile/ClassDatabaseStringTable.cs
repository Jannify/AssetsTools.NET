using System;
using System.Collections.Generic;
using System.Text;
using AssetsTools.NET.Atomic.Helper;

namespace AssetsTools.NET.Atomic
{
    public class ClassDatabaseStringTable
    {
        public ConcurrentList<string> Strings { get; set; }

        public void Read(AssetsFileReader reader)
        {
            int stringCount = reader.ReadInt32();
            Strings = new ConcurrentList<string>(stringCount);
            for (int i = 0; i < stringCount; i++)
            {
                Strings.Add(reader.ReadString());
            }
        }

        public ushort AddString(string str)
        {
            int index = Strings.IndexOf(str);
            if (index == -1)
            {
                index = Strings.Count;
                Strings.Add(str);
            }
            return (ushort)index;
        }

        /// <summary>
        /// Get a string from the string table.
        /// </summary>
        /// <param name="index">The index of the string in the table.</param>
        /// <returns>The string at that index.</returns>
        public string GetString(ushort index)
        {
            return Strings[index];
        }
    }
}
