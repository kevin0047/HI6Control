from flask import Flask, request, jsonify
import json
import time
import random

app = Flask(__name__)

# 가상의 로봇 상태 데이터
robot_state = {
    "cur_mode": 0,
    "enable_state": 0,
    "is_playback": 0,
    "is_remote_mode": 1,
    "is_ext_start": 0,
    "is_ext_prog_sel": 0,
    "cur_prog_no": 1,
    "cur_step_no": 0,
    "cur_func_no": 0,
    "mov_prog_no": 1,
    "mov_step_no": 0,
    "mov_func_no": 0,
    "spd_lev": 5,
    "manual_spd_max": 250,
    "auto_spd": 80,
    "jog_inch_status": 0,
    "step_execute_unit_status": 0,
    "cont_path": 1
}

# 가상의 로봇 위치 데이터
robot_position = {
    "nsync": 0,
    "_type": "Pose",
    "rx": 0.0,
    "x": 1067.366,
    "ry": 73.248,
    "y": -12.859,
    "rz": -0.69,
    "z": 1609.909,
    "mechinfo": 1,
    "crd": "base",
    "j1": 0.0,
    "j2": 0.0,
    "j3": 0.0,
    "j4": 0.0,
    "j5": 0.0,
    "j6": 0.0
}

# 모터 상태 (0: on, 1: off, 2: busy)
motor_state = 1

# API 버전 반환
@app.route('/api_ver', methods=['GET'])
def api_ver():
    return "5"

# 로봇 정보 반환
@app.route('/project/rgen', methods=['GET'])
def rgen():
    return jsonify(robot_state)

# 모터 상태 확인
@app.route('/project/robot/motor_on_state', methods=['GET'])
def motor_on_state():
    global motor_state
    return jsonify({"_type": "JObject", "val": motor_state})

# 로봇 위치 정보 반환
@app.route('/project/robot/po_cur', methods=['GET'])
def po_cur():
    # 매번 위치가 약간씩 랜덤하게 변동되도록 설정
    robot_position["x"] = 1067.366 + random.uniform(-1.0, 1.0)
    robot_position["y"] = -12.859 + random.uniform(-1.0, 1.0)
    robot_position["z"] = 1609.909 + random.uniform(-1.0, 1.0)
    return jsonify(robot_position)

# 모터 ON
@app.route('/project/robot/motor_on', methods=['POST'])
def motor_on():
    global motor_state
    # 모터 상태를 busy(2)로 변경 후 잠시 대기하고 on(0)으로 변경
    motor_state = 2
    time.sleep(0.5)  # 0.5초 대기
    motor_state = 0
    return jsonify({"_type": "JObject"})

# 기본 경로
@app.route('/', methods=['GET'])
def home():
    return "Hi6 Robot Controller Emulator - Running"

if __name__ == '__main__':
    print("Hi6 로봇 제어기 에뮬레이터 시작 중...")
    print("서버 주소: http://127.0.0.1:8888")
    print("WPF 애플리케이션에서 IP를 127.0.0.1로 설정하여 연결하세요.")
    app.run(host='0.0.0.0', port=8888, debug=True)