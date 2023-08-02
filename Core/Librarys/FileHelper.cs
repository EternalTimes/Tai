using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Librarys
{
    /// <summary>
    /// 文件操作帮助类
    /// </summary>
    public class FileHelper
    {
        ///原来的代码
        /// <summary>
        /// 获取根目录
        /// </summary>
        /// <returns></returns>

        /// public static string GetRootDirectory()
        /// {
        ///     return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        /// }

        /// 更改为文档里Tai文件夹，避免uwp容器封装导致的更新数据丢失

        public static string GetRootDirectory()
        {
            string rootDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tai");
            Directory.CreateDirectory(rootDirectoryPath);
            return rootDirectoryPath;
        }

    }
}
