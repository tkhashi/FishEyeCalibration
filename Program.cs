using Calibration;

//// DNG(ROWデータ)→JPG, リサイズ
//var dngDir = @"D:\DCIM\Camera01\first";
//var saveDir = @"C:\Users\k_tak\work\pythonPlayground\calibrate_fisheye\source\resized";
//var converter = new DngConverter.DngConverter();
//var paths = converter.Convert(dngDir, saveDir, 1 / 8d, "jpg");
//return;

// 補正値算出
var (dim, k, d) = new Calibration.Calibration().Calibrate(@"resources/calibrationImage/fromVideo");

// 動画補正
var distortion = new Distortion(dim, k, d);
var videoDistorter = new VideoDistortion(distortion);
var source = @"resources/distortions/VID_20230409_115300_10_037.mp4";
var saveDir = @"C:\Users\k_tak\OneDrive\デスクトップ\output\videoUnDistort";
videoDistorter.UnDistort(source, saveDir);