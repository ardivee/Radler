using Radler.Constants;
using Radler.Models;
using Radler.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Radler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker _worker = BackgroundWorkerUtil.worker;

        #region OptionValues
        private bool _includePrefabs = false;
        private bool _includeModels = false;
        private bool _ignoreSkybox = false;
        private bool _ignoreClip = false;
        private bool _ignoreCaulk = false;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            InitializeLog();

            _worker.DoWork += _worker_DoWork;
            _worker.ProgressChanged += _worker_ProgressChanged;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
        }

        #region BackgroundWorker
        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CancelBtn.IsEnabled = false;
            BackgroundWorkerUtil.workerEvent = null;

            if(e.Cancelled)
            {
                AppendToLog("Conversion Cancelled!");
            }
        }

        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var userState = (WorkerUserState)e.UserState;

            if(userState != null && userState.Message != null)
                AppendToLog(userState.Message);
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var files = (string[])e.Argument;

            BackgroundWorkerUtil.workerEvent = e;

            _worker.ReportProgress(-1, new WorkerUserState("Start Conversion"));

            foreach (string file in files)
            {
                if (System.IO.Path.GetExtension(file) == ".map")
                    ConvertMap(file);
                else
                    _worker.ReportProgress(-1, new WorkerUserState("Invalid file type!"));
            }

            _worker.ReportProgress(-1, new WorkerUserState("Conversion Done!"));
        }

        #endregion

        #region DragDrop
        private void DragImage_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
        }

        private void DragImage_Drop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if(_worker.IsBusy)
            {
                _worker.ReportProgress(-1, new WorkerUserState("I'm a bit busy now!"));
                return;
            }

            ReadOptionValues();

            if (files.Length > 0)
            {
                _worker.RunWorkerAsync(files);
                CancelBtn.IsEnabled = true;
            }
        }

        #endregion

        private void ConvertMap( string file )
        {
            // Get the Map Source path
            var mapSourcePath = GetMapSourcePath(file);

            var map = RadiantMap.Read(file, mapSourcePath, _includePrefabs, _includeModels);

            _worker.ReportProgress(-1, new WorkerUserState(string.Format("Generating: {0}", System.IO.Path.GetFileNameWithoutExtension(file) + ".obj")));

            var obj = WavefrontObj.CreateFromRadiantMap(map);

            if (_ignoreSkybox)
            {
                _worker.ReportProgress(-1, new WorkerUserState("Removing Skybox from .obj"));
                obj.FilterTextures(MapMaterials.SkyMaterials);
            }

            if (_ignoreClip)
            {
                _worker.ReportProgress(-1, new WorkerUserState("Removing Clip from .obj"));
                obj.FilterTextures(MapMaterials.ClipMaterials);
            }
                
            if (_ignoreCaulk)
            {
                _worker.ReportProgress(-1, new WorkerUserState("Removing Caulk from .obj"));
                obj.FilterTextures(MapMaterials.CaulkMaterials);
            }

            SaveUtil.CheckExportDir();

            _worker.ReportProgress(-1, new WorkerUserState(string.Format("Saving: {0}", System.IO.Path.GetFileNameWithoutExtension(file) + ".obj")));

            obj.SaveFile(SaveUtil.GetMapObjSavePath(file));

            ConvertXModelUtil.ConvertModels(map, file);
        }

        #region Log
        private void AppendToLog(string text)
        {
            LogText.AppendText(string.Format("\n [{0}] # {1}", DateTime.Now.ToLongTimeString(), text));
            LogText.ScrollToEnd();
        }

        private void InitializeLog()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            LogText.Text = string.Format(" [{0}] # Radler {1}.{2}.{3} Initialized", DateTime.Now.ToLongTimeString(), version.Major, version.Minor, version.Build);
        }

        #endregion

        private void ReadOptionValues()
        {
            _includePrefabs = (bool)IncludePrefabs.IsChecked;
            _includeModels = (bool)IncludeModels.IsChecked;
            _ignoreSkybox = (bool)IgnoreSkyBox.IsChecked;
            _ignoreClip = (bool)IgnoreClip.IsChecked;
            _ignoreCaulk = (bool)IgnoreCaulk.IsChecked;
        }

        private static string GetMapSourcePath(string file)
        {
            // Read the path of the map file
            string[] directories = file.Split(System.IO.Path.DirectorySeparatorChar);

            var mapSourcePath = string.Empty;
            // Look if we have a map_source in our path to find prefabs
            if (directories.Length > 0)
            {
                mapSourcePath = directories[0] + "\\";

                // Skip file drive
                for (int i = 1; i < directories.Length; i++)
                {
                    mapSourcePath = System.IO.Path.Combine(mapSourcePath, directories[i]);

                    if (directories[i] == "map_source")
                        break;
                }
            }

            return mapSourcePath;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _worker.ReportProgress(-1, new WorkerUserState("Cancelling Conversion, please wait.."));
            _worker.CancelAsync();
        }
    }
}
