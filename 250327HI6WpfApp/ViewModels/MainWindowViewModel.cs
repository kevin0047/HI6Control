using System.Windows.Input;
using _250327HI6WpfApp.Commands;
using _250327HI6WpfApp.Services;

namespace _250327HI6WpfApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        private readonly IRobotService _robotService;

        public MainWindowViewModel(MainViewModel mainViewModel, ControlViewModel controlViewModel,
                          SettingsViewModel settingsViewModel, IRobotService robotService)
        {
            {
                _robotService = robotService;

                MainViewModel = mainViewModel;
                ControlViewModel = controlViewModel;
                SettingsViewModel = settingsViewModel;

                CurrentViewModel = MainViewModel;

                NavigateMainCommand = new RelayCommand(_ => CurrentViewModel = MainViewModel);
                NavigateControlCommand = new RelayCommand(_ => CurrentViewModel = ControlViewModel);
                NavigateSettingsCommand = new RelayCommand(_ => CurrentViewModel = SettingsViewModel);
                // 비상정지 명령
                EmergencyStopCommand = new RelayCommand(_ => ExecuteEmergencyStop());
            }
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        private void ExecuteEmergencyStop()
        {
            try
            {
                string response = _robotService.SendPostRequest("/project/robot/emergency_stop", "{}");
                MainViewModel.AppendToResponse("비상정지 요청 결과:\n" + _robotService.FormatJson(response));
            }
            catch (Exception ex)
            {
                MainViewModel.AppendToResponse("비상정지 오류 발생: " + ex.Message);
            }
        }

        public MainViewModel MainViewModel { get; }
        public ControlViewModel ControlViewModel { get; }
        public SettingsViewModel SettingsViewModel { get; }

        public ICommand NavigateMainCommand { get; }
        public ICommand NavigateControlCommand { get; }
        public ICommand NavigateSettingsCommand { get; }
        public ICommand EmergencyStopCommand { get; }
    }
}