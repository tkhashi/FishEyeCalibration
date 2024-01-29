using ImageMagick;

namespace DngConverter;

/// <summary>
/// DNG(ROWデータ)を変換
/// </summary>
public class DngConverter
{
    public IEnumerable<string> Convert(string dngDir, string saveDir, double resizedScale, string extension)
    {
        // RAWデータをpngに変換
        foreach (var dng in Directory.GetFiles(dngDir).Where(x => Path.GetExtension(x) == ".dng"))
        {
            var fileName = Path.ChangeExtension(dng, extension);
            var outputPath = Path.Combine(saveDir, fileName);
            DngToPng(dng, outputPath, resizedScale);

            yield return outputPath;
        }
    }

    // ROW→png
    // Resize
    private static void DngToPng(string inputPath, string outputPath, double resizeScale)
    {
        using var image = new MagickImage(inputPath);
        image.Format = MagickFormat.Png;
        image.Resize((int) (image.Width * resizeScale), (int) (image.Height * resizeScale));
        image.Write(outputPath);
    }
}