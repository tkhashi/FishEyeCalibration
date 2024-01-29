using OpenCvSharp;

namespace Calibration
{
    public class Distortion
    {
        public Distortion(Size dim, double[,] k, double[,] d)
        {
            _dim = dim;
            _k = InputArray.Create(k);
            _d = InputArray.Create(d);
        }

        public Distortion()
        {
        }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        private readonly Size _dim = new(748, 748);

        /// <summary>
        /// カメラ行列
        /// </summary>
        private readonly InputArray _k = InputArray.Create(new[,]
        {
            // csharpで算出した値
            { 199.0118, 0.0000, 372.6305 },
            { 0.0000, 199.3031, 376.0577 },
            { 0.0000, 0.0000, 1.0000 }

            // pythonで算出した値
            //{ 198.8740791638536, 0.0, 372.60696548881566 },
            //{ 0.0, 199.164517591797, 376.11482369572565 },
            //{ 0.0, 0.0, 1.0 }
        }, MatType.CV_64FC1);

        /// <summary>
        /// 歪み係数
        /// </summary>
        private readonly InputArray _d = InputArray.Create(new[,]
        {
            // csharpで算出した値
            { 0.1065 },
            { -0.0647 },
            { 0.0491 },
            { -0.0168 },

            // pythonで算出した値
            //{ 0.11051804040817755 },
            //{ -0.0739384081419349 },
            //{ 0.05755843084810656 },
            //{ -0.019397708044220068 }
        }, MatType.CV_64FC1);

        public void ResizeDim(Mat target)
        {
            target.Resize(_dim.Width, _dim.Height);
        }

        private void UnDistort(string imgPath)
        {
            using var img = Cv2.ImRead(imgPath, ImreadModes.Grayscale);
            using var map1 = new Mat();
            using var map2 = new Mat();
            Cv2.FishEye.InitUndistortRectifyMap(_k, _d, Mat.Eye(3, 3, MatType.CV_64FC1), _k, _dim, MatType.CV_16SC2,
                map1,
                map2);
            using var undistortedImg = img.Remap(map1, map2, InterpolationFlags.Linear, BorderTypes.Constant);

            var outputDir = @"C:\Users\k_tak\OneDrive\デスクトップ\output\second";
            var outputPath = Path.Combine(outputDir, "out_sharp_" + Path.GetFileName(imgPath));
            Cv2.ImWrite(outputPath, undistortedImg);
        }

        public void Do()
        {
            var images =
                Directory.GetFiles(@"C:\Users\k_tak\work\pythonPlayground\calibrate_fisheye\source\second_resized");
            foreach (var p in images)
            {
                UnDistort(p);
            }
        }

        public Mat UnDistort(Mat img)
        {
            using var map1 = new Mat();
            using var map2 = new Mat();
            var eye = Mat.Eye(3, 3, MatType.CV_64FC1);
            Cv2.FishEye.InitUndistortRectifyMap(_k, _d, eye, _k, _dim, MatType.CV_16SC2, map1, map2);
            var undistortedImg = img.Remap(map1, map2, InterpolationFlags.Linear, BorderTypes.Constant);

            return undistortedImg;
        }
    };
};