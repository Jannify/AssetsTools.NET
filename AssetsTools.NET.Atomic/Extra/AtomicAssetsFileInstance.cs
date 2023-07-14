using System;
using System.Collections.Concurrent;
using System.IO;

namespace AssetsTools.NET.Atomic
{
    /// <summary>
    /// A wrapper around an <see cref="AssetsFile"/> with information such as the path to the file
    /// (used for handling dependencies) and the bundle it belongs to.
    /// </summary>
    public class AtomicAssetsFileInstance
    {
        /// <summary>
        /// The full path to the file. This path can be fake if it is not from disk.
        /// </summary>
        public volatile string path;
        /// <summary>
        /// The name of the file. This is the file name part of the path.
        /// </summary>
        public volatile string name;
        /// <summary>
        /// The bundle this file is a part of, if there is one.
        /// </summary>
        public AtomicBundleFileInstance parentBundle = null;

        private readonly AssetsFile file;
        private readonly object fileLocker = new object();

        internal ConcurrentDictionary<int, AtomicAssetsFileInstance> dependencyCache;

        public AtomicAssetsFileInstance(Stream stream, string filePath)
        {
            path = Path.GetFullPath(filePath);
            name = Path.GetFileName(path);
            file = new AssetsFile();
            file.Read(new AssetsFileReader(stream));
            dependencyCache = new ConcurrentDictionary<int, AtomicAssetsFileInstance>();
        }
        public AtomicAssetsFileInstance(FileStream stream)
        {
            path = stream.Name;
            name = Path.GetFileName(path);
            file = new AssetsFile();
            file.Read(new AssetsFileReader(stream));
            dependencyCache = new ConcurrentDictionary<int, AtomicAssetsFileInstance>();
        }

        public void AccessFileVolatile(Action<AssetFile> action)
        {
            lock (fileLocker)
            {
                action.Invoke(file);
            }
        }

        public void AccessStreamVolatile(Action<Stream> action)
        {
            lock (fileLocker)
            {
                action.Invoke(file.Reader.BaseStream);
            }
        }

        public void Close()
        {
            lock (fileLocker)
            {
                file.Close();
            }
        }

        public AtomicAssetsFileInstance GetDependency(AssetsManager am, int depIdx)
        {
            if (!dependencyCache.ContainsKey(depIdx) || dependencyCache[depIdx] == null)
            {
                string depPath;

                lock (fileLocker)
                {
                    depPath = file.Metadata.Externals[depIdx].PathName;
                }

                if (depPath == string.Empty)
                {
                    return null;
                }

                if (!am.FileLookup.TryGetValue(am.GetFileLookupKey(depPath), out AtomicAssetsFileInstance inst))
                {
                    string pathDir = Path.GetDirectoryName(path);
                    string absPath = Path.Combine(pathDir, depPath);
                    string localAbsPath = Path.Combine(pathDir, Path.GetFileName(depPath));

                    if (File.Exists(absPath))
                    {
                        dependencyCache[depIdx] = am.LoadAssetsFile(absPath, true);
                    }
                    else if (File.Exists(localAbsPath))
                    {
                        dependencyCache[depIdx] = am.LoadAssetsFile(localAbsPath, true);
                    }
                    else if (parentBundle != null)
                    {
                        dependencyCache[depIdx] = am.LoadAssetsFileFromBundle(parentBundle, depPath, true);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    dependencyCache[depIdx] = inst;
                }
            }
            return dependencyCache[depIdx];
        }
    }
}
