using System.Collections;
using System.Diagnostics;

namespace ArchiveMaster.ViewModels.FileSystem
{
    [DebuggerDisplay("{Name}   {Count}个子目录，{Files.Count}个文件")]
    public class FileSystemTree : IReadOnlyList<FileSystemTree>
    {
        private FileSystemTree(FileSystemTree parent, string name) 
        {
            Parent = parent;
            Name= name;
        }
        public static FileSystemTree CreateRoot()
        {
            return new FileSystemTree(null, null);
        }

        public FileSystemTree AddChild(string name)
        {
            var subTree = new FileSystemTree(this, name);
            Directories.Add(subTree);
            return subTree;
        }
        public FileSystemTree AddFile(string name)
        {
            var file=new FileSystemTree(this, name);
            file.IsFile = true;
            Files.Add(file);
            return file;
        }


        public FileSystem.DiscFile File { get; set; }
        public bool IsFile { get; private set; } = false;

        public List<FileSystemTree> Files { get; private set; } = new List<FileSystemTree>();
        public List<FileSystemTree> Directories { get; private set; }=new List<FileSystemTree>();

        public IEnumerable<FileSystemTree> All => Directories.Concat(Files);

        public bool IsEmpty
        {
            get
            {
                return (Files == null || Files.Count == 0) && (Directories == null || Directories.Count == 0);
            }
        }

        public int Count => Directories.Count;


        public IReadOnlyList<FileSystemTree> GetAllFiles()
        {
            List<FileSystemTree> files = new List<FileSystemTree>();
            Get(this);
            return files.AsReadOnly();

            void Get(FileSystemTree tree)
            {
                if (tree.Files != null && tree.Files.Count > 0)
                {
                    files.AddRange(tree.Files);
                }
                if (tree.Directories != null && tree.Directories.Count > 0)
                {
                    foreach (var subTree in tree)
                    {
                        Get(subTree);
                    }
                }
            }
        }

        public FileSystemTree Parent { get; private set; }
        public string Name { get; }

        public FileSystemTree this[int index] => Directories[index];

        public IEnumerator<FileSystemTree> GetEnumerator() => new FreeFileSystemTreeEnumerator(All.ToList());

        public class FreeFileSystemTreeEnumerator : IEnumerator<FileSystemTree>
        {
            public FreeFileSystemTreeEnumerator(List<FileSystemTree> files)
            {
                FileDirs = files;
            }

            private int currentIndex = -1;
            public List<FileSystemTree> FileDirs { get; private set; }
            public FileSystemTree Current => FileDirs[currentIndex];

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                currentIndex++;
                return currentIndex < FileDirs.Count;
            }

            public void Reset() => currentIndex = -1;

            public void Dispose()
            {
            }
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
