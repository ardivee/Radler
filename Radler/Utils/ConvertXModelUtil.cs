using Radler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radler.Utils
{
    public static class ConvertXModelUtil
    {
        private static BackgroundWorker _worker = BackgroundWorkerUtil.worker;

        public static void ConvertModels(RadiantMap map, string file)
        {
            int vertCount = 0;
            int converted = 1;
            int total = map._xmodels.Count;

            using (StreamWriter streamWriter = new StreamWriter(SaveUtil.GetModelsObjSavePath(Path.GetFileNameWithoutExtension(file) + "_models")))
            {
                streamWriter.WriteLine("# Exported using Radler\n\n");

                foreach (var xModel in map._xmodels)
                {
                    if(_worker.CancellationPending)
                    {
                        if(BackgroundWorkerUtil.workerEvent != null)
                        {
                            BackgroundWorkerUtil.workerEvent.Cancel = true;
                            break;
                        }
                    }

                    if (File.Exists(xModel.FilePath))
                    {
                        _worker.ReportProgress(-1, new WorkerUserState(string.Format("Converting: [{1} / {2}] {0}", xModel.Name, converted, total)));

                        using (XAssetFile xFile = new XAssetFile(xModel.FilePath))
                        {
                            List<string> verts = new List<string>();
                            List<string> faces = new List<string>();

                            List<string> face = null;

                            bool readVertOffset = false;
                            bool readTriangle = false;
                            int triangleVertCount = 0;

                            int modelVertCount = 0;

                            foreach (XBlock fileBlock in xFile.FileBlocks)
                            {
                                if (readVertOffset && fileBlock.TextID == "OFFSET")
                                {
                                    Tuple<float, float, float> blockData = (Tuple<float, float, float>)fileBlock.BlockData;

                                    Vector offset = xModel.Origin;

                                    Vector v = new Vector(blockData.Item1, blockData.Item2, blockData.Item3);

                                    // Z X Y order else rotation is wrong
                                    VectorUtil.RotateZ(v, xModel.Angles.Z, false);
                                    VectorUtil.RotateX(v, xModel.Angles.X, true);
                                    VectorUtil.RotateY(v, xModel.Angles.Y, false);

                                    verts.Add(string.Format("v {0} {1} {2}", (v.X + offset.X), (v.Y + offset.Y), (v.Z + offset.Z)));

                                    modelVertCount++;

                                    readVertOffset = false;
                                }

                                if (readTriangle && fileBlock.TextID == "VERT")
                                {
                                    face.Add((int.Parse(fileBlock.BlockData.ToString()) + 1 + vertCount).ToString());

                                    triangleVertCount++;

                                    if (triangleVertCount == 3)
                                    {
                                        triangleVertCount = 0;

                                        if (face != null)
                                        {
                                            // swap to fix normals
                                            string z = face[2];
                                            face[2] = face[1];
                                            face[1] = z;

                                            StringBuilder builder = new StringBuilder();

                                            foreach (string f in face)
                                            {
                                                builder.Append(f + " ");
                                            }

                                            faces.Add("f " + builder.ToString().TrimEnd());
                                        }

                                        readTriangle = false;
                                    }
                                }

                                if (fileBlock.TextID == "VERT")
                                    readVertOffset = true;


                                if (fileBlock.TextID == "TRI")
                                {
                                    readTriangle = true;
                                    face = new List<string>();
                                }
                            }

                            streamWriter.WriteLine(string.Format("o {0}", xModel.Name));

                            foreach (string vert in verts)
                            {
                                streamWriter.WriteLine(vert);
                            }

                            streamWriter.WriteLine("");

                            foreach (string f in faces)
                            {
                                streamWriter.WriteLine(f);
                            }

                            streamWriter.WriteLine("");

                            vertCount = vertCount + modelVertCount;
                        }
                    }

                    converted++;
                }
            }
        }

        public static void GetXModelFilePaths(RadiantMap map)
        {
            var BlackOps3Root = Environment.GetEnvironmentVariable("TA_TOOLS_PATH");

            // These DLL's are needed for System.Data.SQLite
            string[] requiredDLLs = { "sqlite64r.dll", "icudt64r53.dll", "icuin64r53.dll", "icuuc64r53.dll" };

            foreach (string dll in requiredDLLs)
            {
                if (!File.Exists(dll))
                {
                    _worker.ReportProgress(-1, new WorkerUserState(string.Format("{0} not found.", dll)));
                    _worker.ReportProgress(-1, new WorkerUserState(string.Format("Copying {0}.", dll)));
                    File.Copy(Path.Combine(BlackOps3Root, "bin", dll), dll, true);
                }
            }

            var connection = new SQLiteConnection(@"data source=" + Path.Combine(BlackOps3Root, "gdtdb", "gdt.db"));
            connection.Open();

            foreach (var xModel in map._xmodels)
            {
                // Get model ID
                var idCommand = new SQLiteCommand(string.Format("SELECT PK_id FROM _entity WHERE name='{0}';", xModel.Name), connection);

                var id = idCommand.ExecuteScalar();

                if (id != null)
                {
                    string mId = id.ToString();

                    // Get model Path
                    var fileNameCommand = new SQLiteCommand(string.Format("SELECT filename FROM xmodel where PK_id='{0}';", mId), connection);
                    var result = fileNameCommand.ExecuteScalar();

                    // Make sure we have a result
                    if (result != null)
                    {
                        string fileName = result.ToString();
                        xModel.FilePath = Path.Combine(BlackOps3Root, "model_export", fileName); ;
                    }
                    else
                    {
                        _worker.ReportProgress(-1, new WorkerUserState(string.Format("XModel: {0} not found in GDTDB.", xModel.Name)));
                    }
                }
                else
                {
                    _worker.ReportProgress(-1, new WorkerUserState(string.Format("XModel: {0} not found in GDTDB.", xModel.Name)));
                }
            }

            connection.Close();
        }
    }
}
