using System;
using System.Drawing;
//using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace VPJCountDetection
{
    public class Engine
    {
        public static object BoundingBoxes;
        public static int Boxnm;
        static float threshold = 0.70f;
        public const int featuresPerBox = 5;
        public const int rowCount = 13, columnCount = 13;

        public static Bitmap testImage;
        public static Bitmap smpimg3;
        public static Bitmap smpimg2;
        private static readonly (float x, float y)[] boxAnchors = { (0.573f, 0.677f), (1.87f, 2.06f), (3.34f, 5.47f), (7.88f, 3.53f), (9.77f, 9.17f) };


        
        internal static void Run()
        {
            testImage = VPJCountDetection.smpimg;

            //Load sample data
            var sampleData = new MLModel1_ConsoleApp5.MLModel1.ModelInput()
            {
                ImageSource = @"C:\ImageMLProjects\RTSPTrials\RTSPClientNetCore\image.jpg",
            };


            //Load model and predict output
            var result = MLModel1_ConsoleApp5.MLModel1.Predict(sampleData);





            var originalWidth = testImage.Width;
            var originalHeight = testImage.Height;
            Boxnm = 0;
            foreach (var boundingBox in result.BoundingBoxes)

            {

                if (boundingBox.Score > threshold)
                {
                    Boxnm = Boxnm + 1;
                    float x = Math.Max(boundingBox.Left, 0);
                    float y = Math.Max(boundingBox.Top, 0);
                    float w1 = (boundingBox.Right - boundingBox.Left);
                    float h1 = (boundingBox.Bottom - boundingBox.Top);
                    float width = Math.Min(originalWidth - x, w1);
                    float height = Math.Min(originalHeight - y, h1);

                    // fit to current image size
                    x = originalWidth * x / ImageSettings.imageWidth;
                    y = originalHeight * y / ImageSettings.imageHeight;
                    width = originalWidth * width / ImageSettings.imageWidth;
                    height = originalHeight * height / ImageSettings.imageHeight;
                    //smpimg2 = testImage;
                    using (var graphics = Graphics.FromImage(testImage))
                    {
                        graphics.DrawRectangle(new Pen(Color.Red, 3), x, y, width, height);
                        graphics.DrawString(boundingBox.Label, new Font(FontFamily.GenericMonospace, 50f), Brushes.Red, x + 5, y + 5);
                    }



                }


            }

            // Boxnm = result.BoundingBoxes.Length;

            testImage.Save(@"C:\ImageMLProjects\RTSPTrials\RTSPClientNetCore\predicted.jpg");
            smpimg2 = testImage;


            if (Boxnm == VPJCountDetection.Bountlecount)
            {
            }
            else
            {
                VPJCountDetection.runstate = false;
                VPJCountDetection.Badcount = VPJCountDetection.Badcount + 1;

            }








        }






    }
}
