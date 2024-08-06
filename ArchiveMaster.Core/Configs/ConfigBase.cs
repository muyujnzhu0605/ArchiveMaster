using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public abstract class ConfigBase:ObservableObject
    {
        public abstract void Check();
        
        protected static void CheckEmpty(object value, string name)
        {
            if (value == null || value is string s && string.IsNullOrWhiteSpace(s))
            {
                throw new Exception($"{name}为空");
            }
        }
        
        protected static void CheckFile(string filePath, string name)
        {
            CheckEmpty(filePath, name);
            if (!File.Exists(filePath))
            {
                throw new Exception($"{name}不存在");
            }
        }
        protected static void CheckDir(string dirPath, string name)
        {
            CheckEmpty(dirPath, name);
            if (!File.Exists(dirPath))
            {
                throw new Exception($"{name}不存在");
            }
        }
    }
}
