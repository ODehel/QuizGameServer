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
using QuizGame.Presentation.Wpf.ViewModels;

namespace QuizGame.Presentation.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Initialiser le ViewModel
        var viewModel = new BuzzerViewModel();
        DataContext = viewModel;

        // Initialiser les services de buzzer au chargement
        Loaded += (s, e) =>
        {
            var app = (App)Application.Current;
            app.InitializeBuzzerServices("192.168.1");
            
            var buzzerManager = app.BuzzerManager;
            if (buzzerManager != null)
            {
                viewModel.Initialize(buzzerManager);
            }
        };
    }
}