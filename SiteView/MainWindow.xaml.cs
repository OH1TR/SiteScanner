using System;
using System.Collections.Generic;
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

using System.IO;

namespace SiteView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] Directories;
        int pos = 0;
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Directories = Directory.GetDirectories(@"D:\ScannerResults2\dump2");
            pos = 0;
            while (!File.Exists(Directories[pos] + "\\image.jpg") && pos < Directories.Length)
                pos++;

            iImage.Source = new BitmapImage(new Uri(Directories[pos] + "\\image.jpg"));
        }



        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.PageDown)
            {
                if (pos < Directories.Length - 1)
                    pos++;

                while (!File.Exists(Directories[pos] + "\\image.jpg") && pos < Directories.Length - 1)
                    pos++;

                ShowImage();
            }
            if (e.Key == Key.PageUp) // The Arrow-Down key
            {
                if (pos > 0)
                    pos--;

                while (!File.Exists(Directories[pos] + "\\image.jpg") && pos > 0)
                    pos--;

                ShowImage();
            }

            if (e.Key == Key.Escape)
                bBtn.Focus();

        }

        void ShowImage()
        {
            if (File.Exists(Directories[pos] + "\\image.jpg"))
            {
                iImage.Source = new BitmapImage(new Uri(Directories[pos] + "\\image.jpg"));
            }
            if (File.Exists(Directories[pos] + "\\workitem.json"))
            {
                tbData.Text = File.ReadAllText(Directories[pos] + "\\workitem.json");
            }
        }

        private void tbData_KeyDown(object sender, KeyEventArgs e)
        {
            //e.Handled = false;
        }
    }
}
