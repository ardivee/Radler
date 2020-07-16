using Radler.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Radler.Models
{
    public class RadiantMap
    {
        private static BackgroundWorker _worker = BackgroundWorkerUtil.worker;

        private List<Brush> _brushes;
        private List<Patch> _patches;
        public List<XModel> _xmodels;
        public Brush[] Brushes => _brushes.ToArray();
        public Patch[] Patches => _patches.ToArray();

        // Constructor that creates a radiant map object.
        public RadiantMap()
        {
            _brushes = new List<Brush>();
            _patches = new List<Patch>();
            _xmodels = new List<XModel>();
        }

        // Adds a brush to the radiant map.
        public void Add(Brush brush)
        {
            _brushes.Add(brush);
        }

        // Adds a patch to the radiant map.
        public void Add(Patch patch)
        {
            _patches.Add(patch);
        }

        // Returns a string format of the entire radiant map.
        public override string ToString()
        {
            string res = "";

            for (int i = 0; i < Brushes.Length; ++i)
            {
                res += "Brush " + i + ":\n";
                foreach (ClippingPlane f in Brushes[i].ClippingPlanes)
                    res += f + "\n";
            }

            return res;
        }

        // Reads a .map file to our radiant map object.
        public static RadiantMap Read(string file, string mapSourcePath, bool includePrefabs, bool includeModels)
        {
            RadiantMap map = new RadiantMap();

            _worker.ReportProgress(-1, new WorkerUserState(string.Format("Processing: {0}, please be patient!", Path.GetFileName(file))));

            // Set default origin and angles
            Vector v = new Vector(0, 0, 0);
            Vector a = new Vector(0, 0, 0);

            // Parse the map
            Parse(map, file, mapSourcePath, includePrefabs, includeModels, v, a);
            _worker.ReportProgress(-1, new WorkerUserState(string.Format("{0} brushes found", map._brushes.Count)));
            _worker.ReportProgress(-1, new WorkerUserState(string.Format("{0} patches found", map._patches.Count)));

            if (includeModels)
            {
                _worker.ReportProgress(-1, new WorkerUserState(string.Format("{0} models found.", map._xmodels.Count)));

                ConvertXModelUtil.GetXModelFilePaths(map); 
            }

            return map;
        }

        private static void Parse(RadiantMap map, string file, string mapSourcePath, bool includePrefabs, bool includeModels, Vector offset, Vector angles)
        {
            if (!File.Exists(file))
            {
                _worker.ReportProgress(-1, new WorkerUserState(string.Format("Error: {0} not found", Path.GetFullPath(file))));
                return;
            }

            _worker.ReportProgress(-1, new WorkerUserState(string.Format("Reading: {0}", Path.GetFileName(file))));

            bool inBrush = false;
            bool inPatch = false;
            List<string> brushLines = null;

            int openBrackets = 0;
            bool startReading = false;
            bool startReadingEntities = false;

            bool inEntity = false;
            bool foundPrefabInEntity = false;
            bool foundModelInEntity = false;

            List<string> entityLines = null;
            List<Prefab> prefabs = new List<Prefab>();

            string guidPattern = "guid \"{([A-Za-z0-9]+)-([A-Za-z0-9]+)-([A-Za-z0-9]+)-([A-Za-z0-9]+)-([A-Za-z0-9]+)}\"";
            Regex guidRegex = new Regex(guidPattern, RegexOptions.IgnoreCase);

            foreach (var mapLine in File.ReadLines(file))
            {
                if (_worker.CancellationPending)
                {
                    if (BackgroundWorkerUtil.workerEvent != null)
                    {
                        BackgroundWorkerUtil.workerEvent.Cancel = true;
                        break;
                    }
                }


                // Skip empty lines.
                if (mapLine.Length < 1)
                    continue;

                // Skip GUID lines
                Match guidMatch = guidRegex.Match(mapLine);

                if (guidMatch.Success)
                    continue;

                // Start reading brush and patch data
                if (startReading)
                {
                    if (inEntity)
                    {
                        if (mapLine.Contains("\"classname\" \"misc_prefab\"") && includePrefabs)
                        {
                            foundPrefabInEntity = true;
                        }

                        if (mapLine.Contains("\"classname\" \"misc_model\"") && includeModels)
                        {
                            foundModelInEntity = true;
                        }

                        entityLines.Add(mapLine);
                    }

                    if (mapLine.Contains("{"))
                    {
                        if (startReadingEntities)
                        {
                            inEntity = true;
                            entityLines = new List<string>();
                        }
                        else
                        {
                            if (!inBrush)
                            {
                                inBrush = true;
                                brushLines = new List<string>();
                                brushLines.Add(mapLine);
                            }
                            else if (!inPatch)
                                inPatch = true;
                        }

                        openBrackets++;
                    }
                    else if (mapLine.Contains("}"))
                    {
                        if (inPatch)
                        {
                            map.Add(Patch.CreateFromCode(brushLines.ToArray(), offset, angles));
                            inPatch = false;
                        }
                        else if (inBrush)
                        {
                            map.Add(Brush.CreateFromCode(brushLines.ToArray(), offset, angles));

                            inBrush = false;
                            brushLines = new List<string>();
                        }


                        if (startReadingEntities)
                        {
                            if (foundPrefabInEntity)
                            {
                                var prefab = new Prefab();

                                foreach (string line in entityLines)
                                {
                                    if (line.Contains("\"model\""))
                                    {
                                        string trimLine = line.TrimStart();
                                        string[] splitLine = trimLine.Split(' ');

                                        string prefabPath = splitLine[1].Replace("\"", "");

                                        prefab.Path = Path.Combine(mapSourcePath, prefabPath);
                                    }

                                    if (line.Contains("\"origin\""))
                                    {
                                        string cleanLine = line.Replace("\"origin\"", "");
                                        cleanLine = cleanLine.Replace("\"", "");
                                        cleanLine = cleanLine.TrimStart();

                                        string[] origin = cleanLine.Split(' ');

                                        prefab.Origin = new Vector(Double.Parse(origin[0], System.Globalization.CultureInfo.InvariantCulture), Double.Parse(origin[1], System.Globalization.CultureInfo.InvariantCulture), Double.Parse(origin[2], System.Globalization.CultureInfo.InvariantCulture));
                                    }

                                    if (line.Contains("\"angles\""))
                                    {
                                        string cleanLine = line.Replace("\"angles\"", "");
                                        cleanLine = cleanLine.Replace("\"", "");
                                        cleanLine = cleanLine.TrimStart();

                                        string[] a = cleanLine.Split(' ');

                                        prefab.Angles = new Vector(Double.Parse(a[0], System.Globalization.CultureInfo.InvariantCulture), Double.Parse(a[1], System.Globalization.CultureInfo.InvariantCulture), Double.Parse(a[2], System.Globalization.CultureInfo.InvariantCulture));
                                    }
                                }

                                VectorUtil.Add(prefab.Origin, offset);
                                VectorUtil.Add(prefab.Angles, angles);

                                prefabs.Add(prefab);
                            }

                            if (foundModelInEntity)
                            {
                                var xModel = new XModel();

                                foreach (string line in entityLines)
                                {
                                    if (line.Contains("\"model\""))
                                    {
                                        string trimLine = line.TrimStart();
                                        string[] splitLine = trimLine.Split(' ');

                                        string model = splitLine[1].Replace("\"", "");

                                        xModel.Name = model;
                                    }

                                    if (line.Contains("\"origin\""))
                                    {
                                        string cleanLine = line.Replace("\"origin\"", "");
                                        cleanLine = cleanLine.Replace("\"", "");
                                        cleanLine = cleanLine.TrimStart();

                                        string[] origin = cleanLine.Split(' ');

                                        xModel.Origin = new Vector(Double.Parse(origin[0], System.Globalization.CultureInfo.InvariantCulture), Double.Parse(origin[1], System.Globalization.CultureInfo.InvariantCulture), Double.Parse(origin[2], System.Globalization.CultureInfo.InvariantCulture));
                                    }

                                    if (line.Contains("\"angles\""))
                                    {
                                        string cleanLine = line.Replace("\"angles\"", "");
                                        cleanLine = cleanLine.Replace("\"", "");
                                        cleanLine = cleanLine.TrimStart();

                                        string[] a = cleanLine.Split(' ');

                                        xModel.Angles = new Vector(Double.Parse(a[0], System.Globalization.CultureInfo.InvariantCulture), Double.Parse(a[1], System.Globalization.CultureInfo.InvariantCulture), Double.Parse(a[2], System.Globalization.CultureInfo.InvariantCulture));
                                    }

                                }

                                VectorUtil.Add(xModel.Origin, offset);
                                VectorUtil.Add(xModel.Angles, angles);

                                map._xmodels.Add(xModel);
                            }

                            // Reset values
                            inEntity = false;
                            foundPrefabInEntity = false;
                            foundModelInEntity = false;
                        }

                        openBrackets--;
                    }
                    else if (inBrush)
                    {
                        brushLines.Add(mapLine);
                    }

                    // We are at the entities part
                    if (openBrackets == 0)
                        startReadingEntities = true;
                }
                else
                {
                    // Start reading brush and patch data
                    if (mapLine.Contains("{"))
                    {
                        startReading = true;
                        openBrackets++;
                    }
                }
            }

            foreach (Prefab prefab in prefabs)
            {
                //Console.WriteLine(string.Format("Entering Prefab: {0} || Angles: {1}", prefab.Path, prefab.Angles));
                Parse(map, prefab.Path, mapSourcePath, includePrefabs, includeModels, prefab.Origin, prefab.Angles);
            }
        }
    }
}
