using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.IO;

namespace AxView.View
{
    /// <summary>
    /// Logique d'interaction pour VisualisationPoulies.xaml
    /// </summary>
    public partial class VisualisationPoulies : UserControl
    {

        public VisualisationPoulies()
        {
            InitializeComponent();
            Messenger.Default.Register<bool>(this, "RafrechirGraphiques", RafrechirGraphiques);
            Messenger.Default.Register<bool>(this, "RafrechirGraphiquesMoyens", RafrechirGraphiquesMoyens);
            Messenger.Default.Register<string>(this, "ScrrenshotGraphiques", ScreenshotGraphiques);
        }

        private void RafrechirGraphiques(bool statu)
        {
            if (statu)
            {
                this.Plot_Vitesse_Instant.InvalidatePlot(statu);
                this.Plot_Angle_Instant.InvalidatePlot(statu);
                this.Plot_Sat_Instant.InvalidatePlot(statu);
            }
        }

        private void RafrechirGraphiquesMoyens(bool statu)
        {
            if (statu)
            {
                this.Plot_Vitesse_Moyenne.InvalidatePlot(statu);
                this.Plot_Angle_Moyen.InvalidatePlot(statu);
                this.Plot_Sat_Moyen.InvalidatePlot(statu); 
            }
        }

        private void ScreenshotGraphiques(string path)
        {
            //sauvegarde les graphiques en .jpg
            FileStream stream = new FileStream(path+".jpg", FileMode.Create);
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(CopyScreen()));
            encoder.Save(stream);
            stream.Close();
        }

        /// <summary>
        /// Prend un screenshot
        /// </summary>
        /// <returns></returns>
        private BitmapSource CopyScreen()
        {
            using (var screenBmp = new Bitmap(
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }
    }
}
