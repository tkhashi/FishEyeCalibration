using OpenCvSharp;

namespace Calibration;

public class VideoDistortion
{
    private readonly Distortion _distorter;

    public VideoDistortion(Distortion distorter)
    {
        _distorter = distorter;
    }

    public void UnDistort(string videoPath, string outputDir)
    {
        Console.WriteLine("Video UnDistorting...");
        using var capture = VideoCapture.FromFile(videoPath);
        var fileName = "out_" + Path.GetFileName(videoPath);
        var filePath = Path.Combine(outputDir, fileName);
        using var writer = new VideoWriter(
            filePath,
            FourCC.FromString(capture.FourCC),
            capture.Fps,
            new Size(capture.FrameWidth, capture.FrameHeight));

        for (var i = 0; i < capture.FrameCount - 1; i++)
        {
            using var frame = capture.RetrieveMat();
            _distorter.ResizeDim(frame);
            using var distorted = _distorter.UnDistort(frame);
            //Cv2.ImShow("distort", frame);
            //Cv2.WaitKey();

            writer.Write(distorted);
        }
    }
}