using System.IO;

namespace AssetsTools.NET.Atomic
{
    public partial class AssetsManager
    {
        public ClassDatabaseFile LoadClassDatabase(Stream stream)
        {
            ClassDatabase = new ClassDatabaseFile();
            ClassDatabase.Read(new AssetsFileReader(stream));
            return ClassDatabase;
        }

        public ClassDatabaseFile LoadClassDatabase(string path)
        {
            return LoadClassDatabase(File.OpenRead(path));
        }

        public ClassDatabaseFile LoadClassDatabaseFromPackage(UnityVersion version)
        {
            return ClassDatabase = ClassPackage.GetClassDatabase(version);
        }

        public ClassDatabaseFile LoadClassDatabaseFromPackage(string version)
        {
            return ClassDatabase = ClassPackage.GetClassDatabase(version);
        }

        public void LoadClassPackage(Stream stream)
        {
            ClassPackage = new ClassPackageFile();
            ClassPackage.Read(new AssetsFileReader(stream));
        }

        public void LoadClassPackage(string path)
        {
            LoadClassPackage(File.OpenRead(path));
        }
    }
}
