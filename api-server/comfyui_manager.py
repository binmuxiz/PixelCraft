import json
import websocket
import urllib.request
import urllib.parse
import logging
import time
import uuid
import json
from config import COMFY_SERVER_ADDRESS
from fastapi import UploadFile
import requests


CLIENT_ID = str(uuid.uuid4())

# 로그 설정
logging.basicConfig(
    level=logging.DEBUG,  # INFO, WARNING, ERROR, DEBUG 중 선택 가능
    format="%(asctime)s [%(levelname)s] %(message)s",
    handlers=[
        logging.FileHandler("./logging/comfyui_log.log", encoding="utf-8"),  # 로그 파일에 기록
        logging.StreamHandler()  # 콘솔에도 출력
    ]
)


def _upload_to_comfy(uf: UploadFile, subfolder: str) -> str:
    """ComfyUI /upload/image 로 업로드 후 저장된 파일명 반환"""

    upload_url = f"https://{COMFY_SERVER_ADDRESS}/upload/image"
    print(upload_url)

    files = {"image": (uf.filename, uf.file, uf.content_type or "application/octet-stream")}
    data  = {"subfolder": subfolder, "type": "input"}
    resp = requests.post(upload_url, files=files, data=data)
    resp.raise_for_status()
    return resp.json()["name"]
    # {"name" : filename, "subfolder": subfolder, "type": image_upload_type}



# ================== ComfyUI Server와 연결 ==================
def connect_comfyui_server():

    try:
        url = f"wss://{COMFY_SERVER_ADDRESS}/ws?clientId={CLIENT_ID}"

        logging.debug(f"Comfyui 서버에 연결 시도 중... " + url)
        ws = websocket.WebSocket()
        ws.connect(url, timeout=30)
        logging.info("✅ WebSocket 연결 성공")
        return ws

    except websocket.WebSocketTimeoutException as e:
        logging.error("⏰ WebSocket 연결 타임아웃 발생", exc_info=True)
    except websocket.WebSocketConnectionClosedException as e:
        logging.error("🔌 WebSocket 연결이 비정상적으로 종료되었습니다.", exc_info=True)
    except ConnectionRefusedError as e:
        logging.error("🚫 서버에 연결할 수 없습니다 (연결 거부)", exc_info=True)
    except Exception as e:
        logging.error("❌ WebSocket 연결 중 알 수 없는 에러 발생", exc_info=True)
    return None


# ======================================== get_images ============================================
def get_outputs(ws, prompt):
    # prompt_data = queue_prompt(prompt)
    # if not prompt_data or 'prompt_id' not in prompt_data:
    #     logging.error("프롬프트 대기열 추가 실패")
    #     return {}
        
    # prompt_id = prompt_data['prompt_id']
    # logging.debug(f"prompt_id: {prompt_id}")
    
    # 1) 워크플로우 실행 요청
    prompt_id = queue_prompt(prompt)['prompt_id']
    print(f"🎬 ComfyUI 실행 시작: prompt_id={prompt_id}")

    output_images = {}
    
    ws.settimeout(60)  
    start_time = time.time()
    
    try:
        timeout = 60  # 최대 대기 시간(초)
        ws.settimeout(timeout) # recv() 하나당 최대 60초
        start_time = time.time()
        
        while True:
            try:
                out = ws.recv()
                if isinstance(out, str):
                    message = json.loads(out)   # dict
                    
                    if message["type"] == "executing":
                        node = message["data"]["node"]
                        if node is not None:
                            print(f"🔄 실행 중 노드: {node}")
                        else:
                            print("✅ 실행 완료")
                            break
            except ws.timeout:
                print("❌ recv 타임아웃")
                break
            except websocket.WebSocketTimeoutException:
                logging.warning("⏰ Websocket timeout!")
                break
            
                
    except Exception as e:
        logging.error(f"워크플로우 실행 중 예외 발생: {e}", exc_info=True)
        return {}
    
    # 히스토리에서 출력 이미지 가져오기 
    history = get_history(prompt_id)[prompt_id]
    outputs = history.get("outputs", {})
    return outputs

    # # 출력이 images인 모든 노드 이미지 데이터 반환
    # output_images = {}
    # for node_id in history.get("outputs", {}):
    #     node_output = history['outputs'][node_id]
        
    #     # 출력이 이미지인 노드만
    #     if "images" in node_output:
    #         img_output = []
    #         for img in node_output['images']:
    #             img_data = get_image(img["filename"], img["subfolder"], img["type"])
    #             img_output.append((img["filename"], img_data))
    #         output_images[node_id] = img_output
            
    # logging.debug(f"✅ 이미지 결과 추출 완료: {len(output_images)} 노드")
    return output_images


def get_image(filename, subfolder, folder_type):
    data = {"filename": filename, "subfolder": subfolder, "type": folder_type}
    url_values = urllib.parse.urlencode(data)
    with urllib.request.urlopen(f"https://{COMFY_SERVER_ADDRESS}/view?{url_values}") as response:
        return response.read()



def get_history(prompt_id):
    with urllib.request.urlopen(f"https://{COMFY_SERVER_ADDRESS}/history/{prompt_id}") as response:
        return json.loads(response.read())


def get_history_all():
    try:
        with urllib.request.urlopen(f"https://{COMFY_SERVER_ADDRESS}/history", timeout=10) as response:
            return json.loads(response.read())
    except Exception as e:
        logging.error(f"전체 히스토리 조회 중 오류: {e}", exc_info=True)
        return {}

def queue_prompt(prompt):
    p = {"prompt": prompt, "client_id": CLIENT_ID}
    data = json.dumps(p).encode('utf-8')
    req =  urllib.request.Request(f"https://{COMFY_SERVER_ADDRESS}/prompt", data=data)
    return json.loads(urllib.request.urlopen(req).read())


