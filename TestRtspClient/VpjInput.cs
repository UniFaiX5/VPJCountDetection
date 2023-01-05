using Microsoft.ML.Transforms.Image;
using System.Drawing;

namespace VPJCountDetection
{
    public struct ImageSettings
    {
        public const int imageHeight = 600;
        public const int imageWidth = 800;
    }

    public class VpjInput
    {
        [ImageType(ImageSettings.imageHeight, ImageSettings.imageWidth)]
        public Bitmap Image { get; set; }
    }
}