using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Radler.Utils
{
    public static class SaveUtil
    {
        public static string APP_ROOT = Application.StartupPath;
        private const string EXPORT_DIR = "export";

        public static void CheckExportDir()
        {
            var exportDir = Path.Combine(APP_ROOT, EXPORT_DIR);

            if (!Directory.Exists(exportDir))
                Directory.CreateDirectory(exportDir);
        }

        public static string GetMapObjSavePath(string file)
        {
            var exportDir = Path.Combine(APP_ROOT, EXPORT_DIR);

            var fileName = Path.GetFileNameWithoutExtension(file);

            return Path.Combine(exportDir, fileName + ".obj");
        }

        public static string GetModelsObjSavePath(string fileName)
        {
            var exportDir = Path.Combine(APP_ROOT, EXPORT_DIR);

            return Path.Combine(exportDir, fileName + ".obj");
        }
    }
}
