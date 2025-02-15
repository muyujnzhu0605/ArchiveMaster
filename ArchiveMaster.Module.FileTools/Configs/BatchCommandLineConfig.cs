using System;
using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public partial class BatchCommandLineConfig : ConfigBase
    {
        public const string PathPlaceholder = "{file}";

        [ObservableProperty]
        private string dir;

        [ObservableProperty]
        private string program;

        [ObservableProperty]
        private string arguments;

        [ObservableProperty]
        private int level = 1;

        [ObservableProperty]
        private BatchTarget target;

        public override void Check()
        {
            CheckDir(Dir, "目录");
            CheckEmpty(Program, "程序");
            CheckEmpty(Arguments, "参数");
            if (!Arguments.Contains(PathPlaceholder))
            {
                throw new Exception("命令行应包含{file}占位符，用于标识文件或目录地址");
            }

            if (Target is BatchTarget.SpecialLevelDirs or BatchTarget.SpecialLevelElements
                or BatchTarget.SpecialLevelFiles)
            {
                if (Level is not (>= 1 and <= 100))
                {
                    throw new Exception("层数应当在1-100之间");
                }
            }
        }
    }
}