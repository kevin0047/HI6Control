using System;
using System.Windows.Input;
using _250327HI6WpfApp.Commands;
using _250327HI6WpfApp.Services;

namespace _250327HI6WpfApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IRobotService _robotService;
        private string _responseText;
        private bool _isConnected;

        private bool _isRobotRunning;

        private bool _isRemoteMode;

        private System.Timers.Timer _statusTimer;

        public MainViewModel(IRobotService robotService)
        {
            _robotService = robotService;

            CheckConnectionCommand = new RelayCommand(_ => CheckConnection());
            GetRobotInfoCommand = new RelayCommand(_ => GetRobotInfo(), _ => IsConnected);
            MotorOnCommand = new RelayCommand(_ => MotorOn(), _ => IsConnected);
            GetPositionCommand = new RelayCommand(_ => GetPosition(), _ => IsConnected);
            ClearResponseCommand = new RelayCommand(_ => ResponseText = string.Empty);
            TestRunCommand = new RelayCommand(_ => TestRun(), _ => IsConnected && !IsRobotRunning);

            // 5초마다 로봇 상태 확인 타이머 설정
            _statusTimer = new System.Timers.Timer(1000);
            _statusTimer.Elapsed += (s, e) =>
            {
                if (IsConnected)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => CheckRobotRunningState());
                }
            };
            _statusTimer.Start();
        }

        public string ResponseText
        {
            get => _responseText;
            set => SetProperty(ref _responseText, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }
        public bool IsRobotRunning
        {
            get => _isRobotRunning;
            set => SetProperty(ref _isRobotRunning, value);
        }
        public bool IsRemoteMode
        {
            get => _isRemoteMode;
            set => SetProperty(ref _isRemoteMode, value);
        }

        public ICommand CheckConnectionCommand { get; }
        public ICommand GetRobotInfoCommand { get; }
        public ICommand MotorOnCommand { get; }
        public ICommand GetPositionCommand { get; }
        public ICommand ClearResponseCommand { get; }
        public ICommand TestRunCommand { get; }
        public void AppendToResponse(string text)
        {
            ResponseText += text + Environment.NewLine + Environment.NewLine;
        }

        private void CheckConnection()
        {
            try
            {
                string response = _robotService.SendGetRequest("/api_ver");

                if (!string.IsNullOrEmpty(response))
                {
                    IsConnected = true;
                    AppendToResponse("연결 성공! API 버전: " + response);

                    // 연결되면 로봇 상태도 확인
                    CheckRobotRunningState();
                }
                else
                {
                    IsConnected = false;
                    AppendToResponse("연결 실패: 응답이 비어있습니다.");
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                AppendToResponse("연결 실패: " + ex.Message);
            }
        }

        private void GetRobotInfo()
        {
            try
            {
                string response = _robotService.SendGetRequest("/project/rgen");
                AppendToResponse("로봇 정보:\n" + _robotService.FormatJson(response));
            }
            catch (Exception ex)
            {
                AppendToResponse("오류 발생: " + ex.Message);
                CheckConnection();
            }
        }

        private void MotorOn()
        {
            try
            {
                string response = _robotService.SendPostRequest("/project/robot/motor_on", "{}");
                AppendToResponse("모터 ON 요청 결과:\n" + _robotService.FormatJson(response));
            }
            catch (Exception ex)
            {
                AppendToResponse("오류 발생: " + ex.Message);
                CheckConnection();
            }
        }

        private void GetPosition()
        {
            try
            {
                string response = _robotService.SendGetRequest("/project/robot/po_cur?crd=0&mechinfo=1");
                AppendToResponse("현재 로봇 위치:\n" + _robotService.FormatJson(response));
            }
            catch (Exception ex)
            {
                AppendToResponse("오류 발생: " + ex.Message);
                CheckConnection();
            }
        }
        // 로봇 상태 확인 메소드
        private void CheckRobotRunningState()
        {
            try
            {
                string response = _robotService.SendGetRequest("/project/rgen");
                var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response);

                // 로봇 기동 상태 확인
                IsRobotRunning = jsonObject.is_playback == 1;

                // 원격 모드 상태 확인
                IsRemoteMode = jsonObject.is_remote_mode == 1;
            }
            catch (Exception ex)
            {
                AppendToResponse("로봇 상태 확인 중 오류: " + ex.Message);
                IsRobotRunning = false;
                IsRemoteMode = false;
            }
        }
        //테스트 기동 메서드
        private void TestRun()
        {
            try
            {
                // 1. 먼저 로봇의 정보 조회 - 현재 툴 번호 포함
                AppendToResponse("테스트 실행 준비 - 로봇 정보 조회 중...");
                string robotInfoResponse = _robotService.SendGetRequest("/project/rgen");
                var robotInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(robotInfoResponse);

                // 현재 툴 번호 가져오기
                int currentToolNo = robotInfo.tool_no;
                AppendToResponse($"현재 설정된 툴 번호: {currentToolNo}");

                // 2. 로봇의 메커니즘 정보 및 관절 정보 조회
                string robotPoseInfo = _robotService.SendGetRequest("/project/robot/po_cur?crd=2");
                var robotJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(robotPoseInfo);

                // 3. 사용 가능한 관절 목록 확인
                var availableJoints = new System.Collections.Generic.List<string>();
                var jointValues = new System.Collections.Generic.Dictionary<string, double>();

                foreach (Newtonsoft.Json.Linq.JProperty property in robotJson)
                {
                    string propName = property.Name;
                    if (propName.StartsWith("j") && char.IsDigit(propName[1]))
                    {
                        availableJoints.Add(propName);
                        jointValues[propName] = (double)property.Value;
                    }
                }

                AppendToResponse($"사용 가능한 관절 정보: {string.Join(", ", availableJoints)}");

                if (availableJoints.Count == 0)
                {
                    AppendToResponse("사용 가능한 관절을 찾을 수 없습니다. 테스트를 중단합니다.");
                    return;
                }

                // 4. 모터 ON 확인
                string motorResponse = _robotService.SendGetRequest("/project/robot/motor_on_state");
                var motorJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(motorResponse);

                if (motorJson.val != 0)
                {
                    // 모터가 꺼져있으면 켜기
                    _robotService.SendPostRequest("/project/robot/motor_on", "{}");
                    AppendToResponse("모터를 켭니다.");
                    System.Threading.Thread.Sleep(1000); // 모터 켜지는 시간 대기
                }

                // 5. 테스트 실행 - 각 관절 순차적으로 테스트
                AppendToResponse("테스트 움직임 시작...");

                // 테스트할 관절 수 제한 (안전을 위해 최대 3개 관절까지만 테스트)
                int maxJointsToTest = Math.Min(availableJoints.Count, 6);

                // 각 관절에 대해 테스트 수행
                for (int jointIndex = 0; jointIndex < maxJointsToTest; jointIndex++)
                {
                    string currentJoint = availableJoints[jointIndex];
                    AppendToResponse($"관절 {currentJoint} 테스트 시작...");

                    // 원래 위치 백업
                    double originalValue = jointValues[currentJoint];

                    // a. 해당 관절 +5도로 이동
                    var newJointValues = new System.Collections.Generic.Dictionary<string, double>(jointValues);
                    newJointValues[currentJoint] = originalValue + 5;

                    // 현재 툴 번호를 사용하도록 명령 구성
                    string moveCommand = BuildMoveCommand(availableJoints, newJointValues, currentToolNo);

                    AppendToResponse($"{currentJoint} +5도 이동 중...");
                    _robotService.SendPostRequest("/project/context/tasks[0]/execute_move",
                        "{\"stmt\": \"" + moveCommand + "\"}");

                    System.Threading.Thread.Sleep(1500); // 움직임 완료 대기

                    // b. 해당 관절 -5도로 이동 (원래보다 -5도)
                    newJointValues[currentJoint] = originalValue - 5;
                    moveCommand = BuildMoveCommand(availableJoints, newJointValues, currentToolNo);

                    AppendToResponse($"{currentJoint} -5도 이동 중...");
                    _robotService.SendPostRequest("/project/context/tasks[0]/execute_move",
                        "{\"stmt\": \"" + moveCommand + "\"}");

                    System.Threading.Thread.Sleep(1500); // 움직임 완료 대기

                    // c. 원래 위치로 복귀
                    newJointValues[currentJoint] = originalValue;
                    moveCommand = BuildMoveCommand(availableJoints, newJointValues, currentToolNo);

                    AppendToResponse($"{currentJoint} 원래 위치로 복귀 중...");
                    _robotService.SendPostRequest("/project/context/tasks[0]/execute_move",
                        "{\"stmt\": \"" + moveCommand + "\"}");

                    System.Threading.Thread.Sleep(1500); // 움직임 완료 대기

                    AppendToResponse($"관절 {currentJoint} 테스트 완료");
                }

                AppendToResponse($"로봇 테스트 완료. {maxJointsToTest}개 관절 테스트됨.");
            }
            catch (Exception ex)
            {
                AppendToResponse("테스트 실행 중 오류 발생: " + ex.Message);
                // 에러 발생 시 안전을 위해 비상정지 호출
                try
                {
                    _robotService.SendPostRequest("/project/robot/emergency_stop", "{}");
                    AppendToResponse("안전을 위해 비상정지를 실행했습니다.");
                }
                catch
                {
                    AppendToResponse("비상정지 실행 실패");
                }
            }
        }

        // 이동 명령어 생성 도우미 메소드 - 툴 번호 파라미터 추가
        private string BuildMoveCommand(System.Collections.Generic.List<string> joints,
                                      System.Collections.Generic.Dictionary<string, double> values,
                                      int toolNumber)
        {
            string moveCommand = $"move SP,spd=10%,accu=0,tool={toolNumber} [";
            for (int i = 0; i < joints.Count; i++)
            {
                moveCommand += values[joints[i]];
                if (i < joints.Count - 1)
                    moveCommand += ",";
            }
            moveCommand += "]";
            return moveCommand;
        }
        public void Dispose()
        {
            _statusTimer?.Stop();
            _statusTimer?.Dispose();
        }
    }
}