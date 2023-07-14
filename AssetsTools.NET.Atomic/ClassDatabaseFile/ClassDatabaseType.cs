using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;

namespace AssetsTools.NET.Atomic
{
    public class ClassDatabaseType
    {
        public volatile int ClassId;
        public volatile ushort Name;
        public volatile ushort BaseName;
        public volatile ClassFileTypeFlags Flags;

        public ClassDatabaseTypeNode EditorRootNode;
        public ClassDatabaseTypeNode ReleaseRootNode;

        /// <summary>
        /// Read the <see cref="ClassDatabaseType"/> with the provided reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        public void Read(AssetsFileReader reader)
        {
            ClassId = reader.ReadInt32();

            Name = reader.ReadUInt16();
            BaseName = reader.ReadUInt16();

            Flags = (ClassFileTypeFlags)reader.ReadByte();

            EditorRootNode = null;
            if (Net35Polyfill.HasFlag(Flags, ClassFileTypeFlags.HasEditorRootNode))
            {
                EditorRootNode = new ClassDatabaseTypeNode();
                EditorRootNode.Read(reader);
            }

            ReleaseRootNode = null;
            if (Net35Polyfill.HasFlag(Flags, ClassFileTypeFlags.HasReleaseRootNode))
            {
                ReleaseRootNode = new ClassDatabaseTypeNode();
                ReleaseRootNode.Read(reader);
            }
        }

        /// <summary>
        /// Get either the release root node or the editor root node. If only release
        /// or only editor is available, that one will be selected regardless of
        /// <paramref name="preferEditor"/>, otherwise it will select editor or release.
        /// </summary>
        /// <param name="preferEditor">Read from the editor version of this type if available?</param>
        /// <returns>The class database type root node.</returns>
        public ClassDatabaseTypeNode GetPreferredNode(bool preferEditor = false)
        {
            if (EditorRootNode != null && ReleaseRootNode != null)
                return preferEditor ? EditorRootNode : ReleaseRootNode;
            else if (EditorRootNode != null)
                return EditorRootNode;
            else if (ReleaseRootNode != null)
                return ReleaseRootNode;
            else
                return null;
        }
    }
}
