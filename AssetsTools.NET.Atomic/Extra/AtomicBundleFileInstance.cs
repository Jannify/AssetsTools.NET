using System;
using System.IO;
using AssetsTools.NET.Atomic.Helper;

namespace AssetsTools.NET.Atomic
{
    public class AtomicBundleFileInstance
    {
        public volatile string path;
        public volatile string name;
        /// <summary>
        /// List of loaded assets files for this bundle.
        /// </summary>
        /// <remarks>
        /// This list does not contain <i>every</i> assets file for the bundle,
        /// instead only the ones that have been loaded so far.
        /// </remarks>
        public ConcurrentList<AtomicAssetsFileInstance> loadedAssetsFiles;

        private readonly AssetBundleFile file;
        private readonly object fileLocker = new object();

        public AtomicBundleFileInstance(Stream stream, string filePath, bool unpackIfPacked = true)
        {
            path = Path.GetFullPath(filePath);
            name = Path.GetFileName(path);
            file = new AssetBundleFile();
            file.Read(new AssetsFileReader(stream));
            if (file.Header != null && file.DataIsCompressed && unpackIfPacked)
            {
                file = BundleHelper.UnpackBundle(file);
            }
            loadedAssetsFiles = new ConcurrentList<AtomicAssetsFileInstance>();
        }

        public AtomicBundleFileInstance(FileStream stream, bool unpackIfPacked = true)
            : this(stream, stream.Name, unpackIfPacked)
        {
        }

        public void AccessFileVolatile(Action<AssetFile> action)
        {
            lock (fileLocker)
            {
                action.Invoke(file);
            }
        }

        public void AccessBundleStreamVolatile(Action<Stream> action)
        {
            lock (fileLocker)
            {
                action.Invoke(file.Reader.BaseStream);
            }
        }

        public void AccessDataStreamVolatile(Action<Stream> action)
        {
            lock (fileLocker)
            {
                action.Invoke(file.DataReader.BaseStream);
            }
        }

        public void Close()
        {
            lock (fileLocker)
            {
                file.Close();
            }
        }
    }
}
