using System.Collections.Concurrent;
using AssetsTools.NET.Atomic.Helper;

namespace AssetsTools.NET.Atomic
{
    public partial class AssetsManager
    {
        public volatile bool UseTemplateFieldCache = false;
        public volatile bool UseMonoTemplateFieldCache = false;
        public volatile bool UseRefTypeManagerCache = false;

        public ClassDatabaseFile ClassDatabase { get; private set; }
        public ClassPackageFile ClassPackage { get; private set; }

        public ConcurrentList<AtomicAssetsFileInstance> Files { get; private set; } = new ConcurrentList<AtomicAssetsFileInstance>();
        public ConcurrentDictionary<string, AtomicAssetsFileInstance> FileLookup { get; private set; } = new ConcurrentDictionary<string, AtomicAssetsFileInstance>();

        public ConcurrentList<AtomicBundleFileInstance> Bundles { get; private set; } = new ConcurrentList<AtomicBundleFileInstance>();
        public ConcurrentDictionary<string, AtomicBundleFileInstance> BundleLookup { get; private set; } = new ConcurrentDictionary<string, AtomicBundleFileInstance>();

        public IMonoBehaviourTemplateGenerator MonoTempGenerator { get; set; } = null;

        private readonly ConcurrentDictionary<int, AssetTypeTemplateField> templateFieldCache = new ConcurrentDictionary<int, AssetTypeTemplateField>();
        private readonly ConcurrentDictionary<AssetTypeReference, AssetTypeTemplateField> monoTemplateFieldCache = new ConcurrentDictionary<AssetTypeReference, AssetTypeTemplateField>();
        private readonly ConcurrentDictionary<AtomicAssetsFileInstance, ConcurrentDictionary<ushort, AssetTypeTemplateField>> monoTypeTreeTemplateFieldCache = new ConcurrentDictionary<AtomicAssetsFileInstance, ConcurrentDictionary<ushort, AssetTypeTemplateField>>();
        private readonly ConcurrentDictionary<AtomicAssetsFileInstance, ConcurrentDictionary<long, AssetTypeTemplateField>> monoCldbTemplateFieldCache = new ConcurrentDictionary<AtomicAssetsFileInstance, ConcurrentDictionary<long, AssetTypeTemplateField>>();
        private readonly ConcurrentDictionary<AtomicAssetsFileInstance, RefTypeManager> refTypeManagerCache = new ConcurrentDictionary<AtomicAssetsFileInstance, RefTypeManager>();

        public void UnloadAll(bool unloadClassData = false)
        {
            UnloadAllAssetsFiles(true);
            UnloadAllBundleFiles();
            MonoTempGenerator?.Dispose();
            if (unloadClassData)
            {
                ClassPackage = null;
                ClassDatabase = null;
            }
        }
    }

    public struct AssetExternal
    {
        public AssetFileInfo info;
        public AssetTypeValueField baseField;
        public AtomicAssetsFileInstance file;
    }
}
