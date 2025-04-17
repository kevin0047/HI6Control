using System.Windows;
using _250327HI6WpfApp.Services;
using _250327HI6WpfApp.ViewModels;

namespace _250327HI6WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // RobotService 생성
            IRobotService robotService = new RobotService();

            // 각 ViewModel 생성
            var mainViewModel = new MainViewModel(robotService);
            var controlViewModel = new ControlViewModel(robotService);
            var settingsViewModel = new SettingsViewModel(robotService);

            // MainWindowViewModel 생성 및 DataContext 설정
            var mainWindowViewModel = new MainWindowViewModel(
        mainViewModel, controlViewModel, settingsViewModel, robotService);

            // DataContext 설정
            this.DataContext = mainWindowViewModel;
        }
    }
}