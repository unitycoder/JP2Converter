# JP2Converter
Windows commandline converter for .jp2 ortho images into jpg/png/tiff with separate XML/JSON metadata.

### Usage
```
  Jp2OrthoConverter.exe <inputPath> [--format jpg|png|tiff] [--out <outputFolder>]
  <inputPath>  = .jp2 file or folder containing .jp2 files (ortho images).
  --format     = Output format (default: jpg).
  --out        = Output folder (default: same folder as input file).
```

### Other tools
- This .jp2 converter can be useful with GeoTiff importer (to use ortho images with these tiles) https://github.com/unitycoder/UnityGeoTiffImporter

### External licenses
- MIT : MaxRev.Gdal.Core, - MaxRev.Gdal.WindowsRuntime.Minimal (https://github.com/MaxRev-Dev/gdal.netcore?tab=MIT-1-ov-file#readme)
