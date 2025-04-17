using _250327HI6WpfApp.Services;

namespace _250327HI6WpfApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IRobotService _robotService;
        private string _ipAddress = "192.168.255.1";
        private string _port = "8888";

        public SettingsViewModel(IRobotService robotService)
        {
            _robotService = robotService;
        }

        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                if (SetProperty(ref _ipAddress, value))
                {
                    _robotService.UpdateConnection(_ipAddress, _port);
                }
            }
        }

        public string Port
        {
            get => _port;
            set
            {
                if (SetProperty(ref _port, value))
                {
                    _robotService.UpdateConnection(_ipAddress, _port);
                }
            }
        }
    }
}