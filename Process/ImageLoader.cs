using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGO.Utility
{
    public class FLImage
    {
        protected System.Drawing.Image image = null;
        protected BitMiracle.LibTiff.Classic.Tiff tiffImage = null;
        List<int[,][]> intImages = new List<int[,][]>();
        List<double[,][]> doubleImages = new List<double[,][]>();

        public static FLImage LoadImageWithDialog()
        {
            var image = new FLImage();
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Filter = "Jpeg(*.jpg)|*.jpg|PNG(*.png)|*.png|Bitmap(*.bmp)|*.bmp|Tiff(*.tif)|*.tif|CSV(*.csv)|*.csv|TXT(*.txt)|*.txt";
            var result = fileDialog.ShowDialog();
            if (result.Value)
            {
                switch (fileDialog.FilterIndex)
                {
                    case 0: //jpg
                    case 1: //png
                    case 2: //bmp
                        {
                            image.image = System.Drawing.Image.FromFile(fileDialog.FileName);
                        }
                        break;
                    case 3: //tif
                        {
                            image.tiffImage = BitMiracle.LibTiff.Classic.Tiff.Open(fileDialog.FileName, "r");
                        }
                        break;
                    case 4: //csv
                    case 5: //txt
                        {

                        }
                        break; 
                }
            }
            return image;
        }
    }
}
