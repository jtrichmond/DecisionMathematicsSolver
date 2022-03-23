using System.Windows;

namespace WPF_code
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //opens new windows depending on user choice, and closes main window
        private void SortMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SortWindow sortWindow = new();
            sortWindow.Show();
            Close();
        }

        private void SimplexMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new SimplexStartWindow().Show();
            Close();
        }

        private void NetworkMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new NetworkStartWindow().Show();
            Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close(); //exits program
        }
    }
}
