using System.IO;

namespace AssetsTools.NET.Atomic
{
    public partial class AssetsManager
    {
        /// <summary>
        /// Load a <see cref="AtomicBundleFileInstance"/> from a stream with a path.
        /// Use the <see cref="FileStream"/> version of this method to skip the path argument.
        /// If the bundle is large, you may want to set <paramref name="unpackIfPacked"/> to false
        /// so you can manually decompress to file.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="path">The path to set on the <see cref="AtomicAssetsFileInstance"/>.</param>
        /// <param name="unpackIfPacked">Unpack the bundle if it's compressed?</param>
        /// <returns>The loaded <see cref="AtomicBundleFileInstance"/>.</returns>
        public AtomicBundleFileInstance LoadBundleFile(Stream stream, string path, bool unpackIfPacked = true)
        {
            AtomicBundleFileInstance bunInst;
            string lookupKey = GetFileLookupKey(path);
            if (BundleLookup.TryGetValue(lookupKey, out bunInst))
                return bunInst;

            bunInst = new AtomicBundleFileInstance(stream, path, unpackIfPacked);
            Bundles.Add(bunInst);
            BundleLookup[lookupKey] = bunInst;

            return bunInst;
        }

        /// <summary>
        /// Load a <see cref="AtomicBundleFileInstance"/> from a stream.
        /// Assigns the <see cref="AtomicBundleFileInstance"/>'s path from the stream's file path.
        /// If the bundle is large, you may want to set <paramref name="unpackIfPacked"/> to false
        /// so you can manually decompress to file.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="unpackIfPacked">Unpack the bundle if it's compressed?</param>
        /// <returns>The loaded <see cref="AtomicBundleFileInstance"/>.</returns>
        public AtomicBundleFileInstance LoadBundleFile(FileStream stream, bool unpackIfPacked = true)
        {
            return LoadBundleFile(stream, Path.GetFullPath(stream.Name), unpackIfPacked);
        }

        /// <summary>
        /// Load a <see cref="AtomicBundleFileInstance"/> from a path.
        /// If the bundle is large, you may want to set <paramref name="unpackIfPacked"/> to false
        /// so you can manually decompress to file.
        /// </summary>
        /// <param name="path">The path of the file to read from.</param>
        /// <param name="unpackIfPacked">Unpack the bundle if it's compressed?</param>
        /// <returns>The loaded <see cref="AtomicBundleFileInstance"/>.</returns>
        public AtomicBundleFileInstance LoadBundleFile(string path, bool unpackIfPacked = true)
        {
            return LoadBundleFile(File.OpenRead(path), unpackIfPacked);
        }

        /// <summary>
        /// Unload an <see cref="AtomicBundleFileInstance"/> by path.
        /// </summary>
        /// <param name="path">The path of the <see cref="AtomicBundleFileInstance"/> to unload.</param>
        /// <returns>True if the file was found and closed, and false if it wasn't found.</returns>
        public bool UnloadBundleFile(string path)
        {
            string lookupKey = GetFileLookupKey(path);
            if (BundleLookup.TryGetValue(lookupKey, out AtomicBundleFileInstance bunInst))
            {
                bunInst.Close();

                foreach (AtomicAssetsFileInstance assetsInst in bunInst.loadedAssetsFiles)
                {
                    assetsInst.Close();
                }

                Bundles.Remove(bunInst);
                BundleLookup.TryRemove(lookupKey, out _);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unload an <see cref="AtomicBundleFileInstance"/>.
        /// </summary>
        /// <param name="bunInst">The <see cref="AtomicBundleFileInstance"/> to unload.</param>
        /// <returns>True if the file was found and closed, and false if it wasn't found.</returns>
        public bool UnloadBundleFile(AtomicBundleFileInstance bunInst)
        {
            bunInst.Close();

            foreach (AtomicAssetsFileInstance assetsInst in bunInst.loadedAssetsFiles)
            {
                UnloadAssetsFile(assetsInst);
            }

            bunInst.loadedAssetsFiles.Clear();

            if (Bundles.Contains(bunInst))
            {
                string lookupKey = GetFileLookupKey(bunInst.path);
                BundleLookup.TryRemove(lookupKey, out _);
                Bundles.Remove(bunInst);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unload all <see cref="AtomicAssetsFileInstance"/>s.
        /// </summary>
        /// <returns>True if there are files that can be cleared, and false if no files are loaded.</returns>
        public bool UnloadAllBundleFiles()
        {
            if (Bundles.Count != 0)
            {
                foreach (AtomicBundleFileInstance bunInst in Bundles)
                {
                    bunInst.Close();

                    foreach (AtomicAssetsFileInstance assetsInst in bunInst.loadedAssetsFiles)
                    {
                        UnloadAssetsFile(assetsInst);
                    }

                    bunInst.loadedAssetsFiles.Clear();
                }
                Bundles.Clear();
                BundleLookup.Clear();
                return true;
            }
            return false;
        }

        private bool IsAssetsFilePreviewSafe(AtomicBundleFileInstance bunInst, int index)
        {
            bool isSafe = false;
            bunInst.AccessFileVolatile((file =>
            {
                AssetBundleDirectoryInfo dirInfo = BundleHelper.GetDirInfo(file, index);
                if (dirInfo.IsReplacerPreviewable)
                {
                    Stream previewStream = dirInfo.Replacer.GetPreviewStream();
                    isSafe =  AssetsFile.IsAssetsFile(new AssetsFileReader(previewStream), 0, previewStream.Length);
                }
                else
                {
                    isSafe =  file.IsAssetsFile(index);
                }
            }));
            return isSafe;
        }

        /// <summary>
        /// Load an <see cref="AtomicAssetsFileInstance"/> from a <see cref="AtomicBundleFileInstance"/> by index.
        /// </summary>
        /// <param name="bunInst">The bundle to load from.</param>
        /// <param name="index">The index of the file in the bundle to load from.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <returns>The loaded <see cref="AtomicAssetsFileInstance"/>.</returns>
        public AtomicAssetsFileInstance LoadAssetsFileFromBundle(AtomicBundleFileInstance bunInst, int index, bool loadDeps = false)
        {

            string assetMemPath = string.Empty;
            bunInst.AccessFileVolatile(file => assetMemPath = Path.Combine(bunInst.path, file.GetFileName(index)));

            string assetLookupKey = GetFileLookupKey(assetMemPath);

            if (!FileLookup.TryGetValue(assetLookupKey, out AtomicAssetsFileInstance fileInst))
            {
                if (IsAssetsFilePreviewSafe(bunInst, index))
                {
                    AtomicAssetsFileInstance assetsInst = null;
                    bunInst.AccessFileVolatile(file =>
                    {
                        file.GetFileRange(index, out long offset, out long length);
                        SegmentStream stream = new SegmentStream(file.DataReader.BaseStream, offset, length);
                        assetsInst = LoadAssetsFile(stream, assetMemPath, loadDeps, bunInst: bunInst);
                    });
                    bunInst.loadedAssetsFiles.Add(assetsInst);
                    return assetsInst;
                }
                return null;
            }
            else
            {
                return fileInst;
            }
        }

        /// <summary>
        /// Load an <see cref="AtomicAssetsFileInstance"/> from a <see cref="AtomicBundleFileInstance"/> by name.
        /// </summary>
        /// <param name="bunInst">The bundle to load from.</param>
        /// <param name="name">The name of the file in the bundle to load from.</param>
        /// <param name="loadDeps">Load all dependencies immediately?</param>
        /// <returns>The loaded <see cref="AtomicAssetsFileInstance"/>.</returns>
        public AtomicAssetsFileInstance LoadAssetsFileFromBundle(AtomicBundleFileInstance bunInst, string name, bool loadDeps = false)
        {
            int index = 0;
            bunInst.AccessFileVolatile(file => index = file.GetFileIndex(name));

            if (index < 0)
                return null;

            return LoadAssetsFileFromBundle(bunInst, index, loadDeps);
        }
    }
}
