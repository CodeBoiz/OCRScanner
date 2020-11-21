using Emgu.CV;
using Microsoft.Win32;
using OCRScanner.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OCRScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string ImgLocation { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            btnDetect.IsEnabled = false;
        }

        private void btnOpenImg_Click(object sender, RoutedEventArgs e)
        {
            //Create a string to hold the file location
            string fileLocation;

            //Create an openfiledialog object to select a photo
            OpenFileDialog dialog = new OpenFileDialog
            {
                InitialDirectory = Directory.GetParent(Directory.GetParent
                (Environment.CurrentDirectory).ToString()).ToString(),
                Title = "Browse Images",

                CheckFileExists = true,
                CheckPathExists = true,

                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (dialog.ShowDialog() == true)
            {
                fileLocation = dialog.FileName;

                Mat input = ImageUtils.ReadInImage(fileLocation);

                imgOutput.Source = ImageUtils.ImageSourceFromBitmap(input.ToBitmap());

                ImgLocation = fileLocation;

                btnDetect.IsEnabled = true;
            }
        }

        private void btnDetect_Click(object sender, RoutedEventArgs e)
        {
            Mat input = ImageUtils.ReadInImage(ImgLocation);

            List<string> output = OCR.RecognizeText(input);

            lstOutput.ItemsSource = output;
        }
    }
}
