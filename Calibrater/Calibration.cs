using OpenCvSharp;

namespace Calibration;

public class Calibration
{
    public (Size dim, double[,] k, double[,] d) Calibrate(string paths)
    {
        Console.WriteLine("Calibrating...");
        // 終了条件
        var subPixCriteria = new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 30, 0.1);

        // mm単位のチェッカーボードマス実測値
        var squareSize = 27.0;
        // チェッカーボードの交点の数(w, h)
        var checkerboardSize = new Size(6, 9);
        var objP = new List<Point3f>();
        for (var i = 0; i < checkerboardSize.Height; i++)
        for (var j = 0; j < checkerboardSize.Width; j++)
        {
            objP.Add(new Point3f(j * (float) squareSize, i * (float) squareSize, 0));
        }

        // 3D(現実)のチェッカーボード交点
        var objPoints = new List<Point3f[]>();
        // 2D画像上のチェッカーボード交点
        var imgPoints = new List<Point2f[]>();

        Size? imgShape = null;
        var images = Directory.GetFiles(paths);
        foreach (var fileName in images)
        {
            using var img = Cv2.ImRead(fileName);
            imgShape ??= img.Size();
            if (imgShape.Value.Height != img.Height || imgShape.Value.Width != img.Width)
            {
                throw new Exception("All images must share the same size.");
            }

            // チェスボードのコーナーを探す
            using var gray = img.CvtColor(ColorConversionCodes.BGR2GRAY);
            var found = Cv2.FindChessboardCorners(gray, checkerboardSize, out var corners,
                ChessboardFlags.AdaptiveThresh | ChessboardFlags.FastCheck | ChessboardFlags.NormalizeImage);

            // コーナーが見つかったら、オブジェクトポイントと画像ポイントを追加（繊細化した後）
            if (found)
            {
                objPoints.Add(objP.ToArray());
                Cv2.CornerSubPix(gray, corners, new Size(3, 3), new Size(-1, -1), subPixCriteria);
                imgPoints.Add(corners);
            }

            Console.WriteLine("FINISHED " + fileName);
        }

        Console.WriteLine("img end");

        var nOk = objPoints.Count;
        var k = new Mat(3, 3, MatType.CV_64FC1);
        var d = new Mat(4, 1, MatType.CV_64FC1);
        var calibrationFlags = (FishEyeCalibrationFlags.RecomputeExtrinsic | FishEyeCalibrationFlags.CheckCond |
                                FishEyeCalibrationFlags.FixSkew);

        // キャリブレーションを実行
        if (imgShape is null) throw new NullReferenceException("画像のサイズがnullです");
        var _ = Cv2.FishEye.Calibrate(
            objPoints.Select(pArr => new Mat(pArr.Length, 1, MatType.CV_32FC3, pArr)).ToArray(),
            imgPoints.Select(pArr => new Mat(pArr.Length, 1, MatType.CV_32FC2, pArr)).ToArray(),
            imgShape.Value, k, d, out var _, out var _, calibrationFlags,
            new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 30, 1e-6)
        );

        Console.WriteLine("Found " + nOk + " valid images for calibration");
        Console.WriteLine("DIM=" + imgShape);
        Console.WriteLine("K=new double[,]");
        PrintArray(ToArr(k));
        Console.WriteLine("D=new double[,]");
        PrintArray(ToArr(d));

        return (imgShape.Value, ToArr(k), ToArr(d));
    }

    private double[,] ToArr(Mat mat)
    {
        // Matからdouble[,]へ変換
        var array = new double[mat.Rows, mat.Cols];
        for (var i = 0; i < mat.Rows; i++)
        for (var j = 0; j < mat.Cols; j++)
            array[i, j] = mat.At<double>(i, j);

        return array;
    }

    // double[,]型の配列を出力するヘルパーメソッド
    private void PrintArray(double[,] array)
    {
        Console.WriteLine("{");
        for (var i = 0; i < array.GetLength(0); i++)
        {
            Console.Write("    { ");
            for (var j = 0; j < array.GetLength(1); j++)
            {
                var text = j < array.GetLength(1) - 1
                    ? $"{array[i, j],10:F4}, "
                    : $"{array[i, j],10:F4} ";
                Console.Write(text);
            }

            var end = i < array.GetLength(0) - 1
                ? "},"
                : "}";
            Console.WriteLine(end);
        }

        Console.WriteLine("}");
    }
}