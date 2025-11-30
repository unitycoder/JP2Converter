// https://github.com/unitycoder/JP2Converter

using System.Text.Json;
using MaxRev.Gdal.Core;
using OSGeo.GDAL;

namespace Jp2OrthoConverter
{
    internal class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    PrintUsage();
                    return 1;
                }

                // Default values
                string format = "jpg"; // jpg, png, tiff
                string? outputFolder = null;

                // First non-option argument is input path
                string? inputPath = null;
                var argList = new List<string>(args);
                for (int i = 0; i < argList.Count; i++)
                {
                    string a = argList[i];

                    if (a.Equals("--format", StringComparison.OrdinalIgnoreCase) && i + 1 < argList.Count)
                    {
                        format = argList[i + 1].ToLowerInvariant();
                        i++;
                    }
                    else if (a.Equals("--out", StringComparison.OrdinalIgnoreCase) && i + 1 < argList.Count)
                    {
                        outputFolder = argList[i + 1];
                        i++;
                    }
                    else if (!a.StartsWith("-", StringComparison.Ordinal))
                    {
                        if (inputPath == null)
                            inputPath = a;
                        else
                        {
                            Console.Error.WriteLine("Only one input path is supported.");
                            return 1;
                        }
                    }
                }

                if (inputPath == null)
                {
                    PrintUsage();
                    return 1;
                }

                if (format != "jpg" && format != "png" && format != "tiff")
                {
                    Console.Error.WriteLine("Unsupported format: " + format);
                    Console.Error.WriteLine("Valid formats: jpg, png, tiff");
                    return 1;
                }

                // Configure GDAL
                GdalBase.ConfigureAll();
                Gdal.AllRegister();

                if (File.Exists(inputPath))
                {
                    if (!inputPath.EndsWith(".jp2", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Error.WriteLine("Input file is not .jp2: " + inputPath);
                        return 1;
                    }

                    ProcessFile(inputPath, outputFolder, format);
                }
                else if (Directory.Exists(inputPath))
                {
                    string[] files = Directory.GetFiles(inputPath, "*.jp2", SearchOption.TopDirectoryOnly);
                    if (files.Length == 0)
                    {
                        Console.Error.WriteLine("No .jp2 files found in folder: " + inputPath);
                        return 1;
                    }

                    foreach (string f in files)
                    {
                        ProcessFile(f, outputFolder, format);
                    }
                }
                else
                {
                    Console.Error.WriteLine("Input path not found: " + inputPath);
                    return 1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("JP2 Ortho Image Converter v0.1");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  Jp2OrthoConverter.exe <inputPath> [--format jpg|png|tiff] [--out <outputFolder>]");
            Console.WriteLine();
            Console.WriteLine("  <inputPath>  = .jp2 file or folder containing .jp2 files (Maanmittauslaitos ortho images).");
            Console.WriteLine("  --format     = Output format: jpg,tiff,png (default: jpg) .");
            Console.WriteLine("  --out        = Output folder (default: same folder as input file).");
            Console.WriteLine();
            Console.WriteLine("More info: https://github.com/unitycoder/JP2Converter");
        }

        private static void ProcessFile(string inputFile, string? outputFolder, string format)
        {
            Console.WriteLine("Processing: " + inputFile);

            Dataset? ds = Gdal.Open(inputFile, Access.GA_ReadOnly);
            if (ds == null)
            {
                Console.Error.WriteLine("GDAL could not open file: " + inputFile);
                return;
            }

            try
            {
                // Build output paths
                string inputDir = Path.GetDirectoryName(inputFile) ?? ".";
                string inputNameNoExt = Path.GetFileNameWithoutExtension(inputFile);

                string outDir = outputFolder ?? inputDir;

                Directory.CreateDirectory(outDir);

                string outExt = format == "tiff" ? ".tif" : "." + format;
                string outputImagePath = Path.Combine(outDir, inputNameNoExt + outExt);
                string outputJsonPath = Path.Combine(outDir, inputNameNoExt + outExt + ".json");

                // Convert image
                ConvertImage(ds, outputImagePath, format);

                // Export metadata as JSON
                var metadata = ExtractMetadata(ds, inputFile, outputImagePath);
                ExportMetadata(metadata, outputJsonPath);

                Console.WriteLine("  -> Image: " + outputImagePath);
                Console.WriteLine("  -> JSON : " + outputJsonPath);
            }
            finally
            {
                ds.Dispose();
            }
        }

        private static void ConvertImage(Dataset sourceDs, string outputImagePath, string format)
        {
            string driverName;
            switch (format)
            {
                case "jpg":
                    driverName = "JPEG";
                    break;
                case "png":
                    driverName = "PNG";
                    break;
                case "tiff":
                    driverName = "GTiff";
                    break;
                default:
                    throw new ArgumentException("Unsupported format: " + format);
            }

            Driver? driver = Gdal.GetDriverByName(driverName);
            if (driver == null)
                throw new InvalidOperationException("GDAL driver not found: " + driverName);

            // Creation options: you can tweak quality here
            string[] options;
            if (driverName == "JPEG")
            {
                options = new[] { "QUALITY=90" };
            }
            else
            {
                options = Array.Empty<string>();
            }

            using Dataset outDs = driver.CreateCopy(
                outputImagePath,
                sourceDs,
                0,
                options,
                null,
                null
            );

            if (outDs == null)
                throw new InvalidOperationException("Failed to create output image: " + outputImagePath);
        }

        private class GeoMetadata
        {
            public string InputFile { get; set; } = "";
            public string OutputFile { get; set; } = "";
            public int Width { get; set; }
            public int Height { get; set; }
            public int BandCount { get; set; }
            public string DataType { get; set; } = "";
            public double[] GeoTransform { get; set; } = Array.Empty<double>();
            public string ProjectionWkt { get; set; } = "";
            public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, Dictionary<string, string>> MetadataDomains { get; set; } =
                new Dictionary<string, Dictionary<string, string>>();
            public GcpInfo[] Gcps { get; set; } = Array.Empty<GcpInfo>();
        }

        private class GcpInfo
        {
            public string Id { get; set; } = "";
            public string Info { get; set; } = "";
            public double Pixel { get; set; }
            public double Line { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
        }

        private static GeoMetadata ExtractMetadata(Dataset ds, string inputFile, string outputFile)
        {
            var meta = new GeoMetadata
            {
                InputFile = Path.GetFullPath(inputFile),
                OutputFile = Path.GetFullPath(outputFile),
                Width = ds.RasterXSize,
                Height = ds.RasterYSize,
                BandCount = ds.RasterCount
            };

            if (ds.RasterCount > 0)
            {
                var dt = ds.GetRasterBand(1).DataType;
                meta.DataType = dt.ToString();
            }

            // GeoTransform
            double[] geoTransform = new double[6];
            ds.GetGeoTransform(geoTransform);
            meta.GeoTransform = geoTransform;

            // Projection
            meta.ProjectionWkt = ds.GetProjectionRef() ?? "";

            // Default metadata (no domain)
            var defaultMeta = ds.GetMetadata("");
            if (defaultMeta != null)
            {
                foreach (string item in defaultMeta)
                {
                    int idx = item.IndexOf('=');
                    if (idx > 0)
                    {
                        string key = item.Substring(0, idx);
                        string value = item.Substring(idx + 1);
                        meta.Metadata[key] = value;
                    }
                }
            }

            // Domain-specific metadata (RPC, GEOLOCATION, IMAGE_STRUCTURE, etc.)
            string[] domains = ds.GetMetadataDomainList() ?? Array.Empty<string>();
            foreach (var domain in domains)
            {
                var dict = new Dictionary<string, string>();
                var arr = ds.GetMetadata(domain);
                if (arr != null)
                {
                    foreach (string item in arr)
                    {
                        int idx = item.IndexOf('=');
                        if (idx > 0)
                        {
                            string key = item.Substring(0, idx);
                            string value = item.Substring(idx + 1);
                            dict[key] = value;
                        }
                    }
                }

                meta.MetadataDomains[domain] = dict;
            }

            // GCPs (if present)
            int gcpCount = ds.GetGCPCount();
            if (gcpCount > 0)
            {
                var gcps = ds.GetGCPs();
                var list = new List<GcpInfo>();
                foreach (var gcp in gcps)
                {
                    list.Add(new GcpInfo
                    {
                        Id = gcp.Id ?? "",
                        Info = gcp.Info ?? "",
                        Pixel = gcp.GCPX,
                        Line = gcp.GCPY,
                        X = gcp.GCPX,
                        Y = gcp.GCPY,
                        Z = gcp.GCPZ
                    });
                }

                meta.Gcps = list.ToArray();
            }

            return meta;
        }

        private static void ExportMetadata(GeoMetadata metadata, string outputJsonPath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(metadata, options);
            File.WriteAllText(outputJsonPath, json);
        }
    }
}
