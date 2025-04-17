using _250327HI6WpfApp.Services;

namespace _250327HI6WpfApp.ViewModels
{
    public class ControlViewModel : ViewModelBase
    {
        private readonly IRobotService _robotService;

        public ControlViewModel(IRobotService robotService)
        {
            _robotService = robotService;
        }

        // 로봇 제어 관련 속성 및 명령어 추가
    }
}