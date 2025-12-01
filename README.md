# JP2Converter
Windows commandline converter for .jp2 ortho images into jpg/png/tiff with separate XML/JSON metadata.

Tested with Maanmittauslaitos ortho images https://asiointi.maanmittauslaitos.fi/karttapaikka/tiedostopalvelu/ortoilmakuva?lang=en

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

### Images

<img width="343" height="343" alt="image" src="https://github.com/user-attachments/assets/b3736ac7-9414-45ef-a0d6-d8c477731b7d" />
<img width="486" height="117" alt="image" src="https://github.com/user-attachments/assets/0143d84b-cd94-4d63-9cd3-b5cfcdfbabec" />
<img width="266" height="243" alt="image" src="https://github.com/user-attachments/assets/03c25739-ba7e-4689-b71b-4721baf6fe0d" />
<img width="500" height="178" alt="image" src="https://github.com/user-attachments/assets/2b6c687f-a111-4bfc-bdc8-29e3d727f415" />

Image used in Unity:<br>
<img width="2560" height="1400" alt="image" src="https://github.com/user-attachments/assets/f493149e-025b-4441-82e3-68d2447a4605" />



