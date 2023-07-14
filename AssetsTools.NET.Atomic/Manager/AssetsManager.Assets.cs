using System.IO;

namespace AssetsTools.NET.Atomic
{
    public partial class AssetsManager
    {
        internal string GetFileLookupKey(string path)
        {
            return Path.GetFullPath(path).ToLower();
        }

        private void LoadAssetsFileDependencies(AtomicAssetsFileInstance fileInst, string path, AtomicBundleFileInstance bunInst)
        {
            if (bunInst == null)
                LoadDependencies(fileInst);
            else
                LoadBundleDependencies(fileInst, bunInst, Path.GetDirectoryName(path));
        }

        /// <summary>
        /// Load an <see cref="AtomicAssetsFileInstance"/> from a stream with a path.
        /// Use the <see cref="FileStream"/> version of this method to skip the path argument.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="path">The path to set on the <see cref="AtomicAssetsFileInstance"/>.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <param name="bunInst">The parent bundle, if one exists.</param>
        /// <returns>The loaded <see cref="AtomicAssetsFileInstance"/>.</returns>
        public AtomicAssetsFileInstance LoadAssetsFile(Stream stream, string path, bool loadDeps = false, AtomicBundleFileInstance bunInst = null)
        {
            string lookupKey = GetFileLookupKey(path);
            if (FileLookup.TryGetValue(lookupKey, out AtomicAssetsFileInstance fileInst))
            {
                if (loadDeps)
                {
                    LoadAssetsFileDependencies(fileInst, path, bunInst);
                }
                return fileInst;
            }
            else
            {
                return LoadAssetsFileCacheless(stream, path, loadDeps, bunInst);
            }
        }

        private AtomicAssetsFileInstance LoadAssetsFileCacheless(Stream stream, string path, bool loadDeps, AtomicBundleFileInstance bunInst = null)
        {
            AtomicAssetsFileInstance fileInst = new AtomicAssetsFileInstance(stream, path);
            fileInst.parentBundle = bunInst;

            string lookupKey = GetFileLookupKey(path);
            FileLookup[lookupKey] = fileInst;
            Files.Add(fileInst);

            if (loadDeps)
            {
                LoadAssetsFileDependencies(fileInst, path, bunInst);
            }
            return fileInst;
        }

        /// <summary>
        /// Load an <see cref="AtomicAssetsFileInstance"/> from a stream.
        /// Assigns the <see cref="AtomicAssetsFileInstance"/>'s path from the stream's file path.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <returns>The loaded <see cref="AtomicAssetsFileInstance"/>.</returns>
        public AtomicAssetsFileInstance LoadAssetsFile(FileStream stream, bool loadDeps = false)
        {
            return LoadAssetsFileCacheless(stream, stream.Name, loadDeps);
        }

        /// <summary>
        /// Load an <see cref="AtomicAssetsFileInstance"/> from a path.
        /// </summary>
        /// <param name="path">The path of the file to read from.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <returns>The loaded <see cref="AtomicAssetsFileInstance"/>.</returns>
        public AtomicAssetsFileInstance LoadAssetsFile(string path, bool loadDeps = false)
        {
            string lookupKey = GetFileLookupKey(path);
            if (FileLookup.TryGetValue(lookupKey, out AtomicAssetsFileInstance fileInst))
                return fileInst;

            return LoadAssetsFile(File.OpenRead(path), loadDeps);
        }

        /// <summary>
        /// Unload an <see cref="AtomicAssetsFileInstance"/> by path.
        /// </summary>
        /// <param name="path">The path of the <see cref="AtomicAssetsFileInstance"/> to unload.</param>
        /// <returns>True if the file was found and closed, and false if it wasn't found.</returns>
        public bool UnloadAssetsFile(string path)
        {
            string lookupKey = GetFileLookupKey(path);
            if (FileLookup.TryGetValue(lookupKey, out AtomicAssetsFileInstance fileInst))
            {
                monoTypeTreeTemplateFieldCache.TryRemove(fileInst, out _);
                monoCldbTemplateFieldCache.TryRemove(fileInst, out _);
                refTypeManagerCache.TryRemove(fileInst, out _);

                Files.Remove(fileInst);
                FileLookup.TryRemove(lookupKey, out _);
                fileInst.Close();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unload an <see cref="AtomicAssetsFileInstance"/>.
        /// </summary>
        /// <param name="fileInst">The <see cref="AtomicAssetsFileInstance"/> to unload.</param>
        /// <returns>True if the file was found and closed, and false if it wasn't found.</returns>
        public bool UnloadAssetsFile(AtomicAssetsFileInstance fileInst)
        {
            fileInst.Close();

            if (Files.Contains(fileInst))
            {
                monoTypeTreeTemplateFieldCache.TryRemove(fileInst, out _);
                monoCldbTemplateFieldCache.TryRemove(fileInst, out _);
                refTypeManagerCache.TryRemove(fileInst, out _);

                string lookupKey = GetFileLookupKey(fileInst.path);
                FileLookup.TryRemove(lookupKey, out _);
                Files.Remove(fileInst);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unload all <see cref="AtomicAssetsFileInstance"/>s.
        /// </summary>
        /// <param name="clearCache">Clear the cache? Recommended if you plan on reopening files later.</param>
        /// <returns>True if there are files that can be cleared, and false if no files are loaded.</returns>
        public bool UnloadAllAssetsFiles(bool clearCache = false)
        {
            if (clearCache)
            {
                templateFieldCache.Clear();
                monoTemplateFieldCache.Clear();
            }

            monoTypeTreeTemplateFieldCache.Clear();
            monoCldbTemplateFieldCache.Clear();
            refTypeManagerCache.Clear();

            if (Files.Count != 0)
            {
                foreach (AtomicAssetsFileInstance assetsInst in Files)
                {
                    assetsInst.Close();
                }
                Files.Clear();
                FileLookup.Clear();
                return true;
            }
            return false;
        }
    }
}
