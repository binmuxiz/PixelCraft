import json
import logging
import image_utils
import comfyui_manager
import os
from config import WF_DIR

from fastapi import UploadFile
from comfyui_manager import _upload_to_comfy, get_outputs, get_image

# 로그 설정
logging.basicConfig(
    level=logging.DEBUG,  # INFO, WARNING, ERROR, DEBUG 중 선택 가능
    format="%(asctime)s [%(levelname)s] %(message)s",
    handlers=[
        logging.FileHandler("./logging/pixel_character_api_debug.log", encoding="utf-8"),  # 로그 파일에 기록
        logging.StreamHandler()  # 콘솔에도 출력
    ]
)



WORKFLOW_1 = "wf_pixel_character.json"
WORKFLOW_2 = "wf_framesheet.json"


"""
[그냥 공부한 내용]
application/octet-stream 이란?
MIME 타입 중 하나
파일이 이미지인지 텍스트인지 명확하지 않을 때 기본값으로 씁니다.

raise_for_status()
requests 라이브러리에서 제공하는 메소드
서버 응답 status_code가 200 OK 범위가 아닐 경우 예외를 던짐
"""


# ==================================== 픽셀 캐릭터 워크플로우 실행 ====================================
def run_pixel_character_workflow(file: UploadFile):
    
    # 1) 워크플로우 로드
    workflow_path = os.path.join(WF_DIR, WORKFLOW_1)
    workflow = get_workflow(workflow_path)
    if not workflow:
        return

    INPUT_NODE_ID = "82"
    OUTPUT_NODE_ID = "205"

    # 2) ComfyUI 서버에 파일 업로드
    uploaded_filename = _upload_to_comfy(file, subfolder="fullbody")
    logging.debug(f"ComfyUI에 파일 업로드: uploaded_filename = {uploaded_filename}")

    # 3) 업로드된 파일명을 LoadImage 노드에 삽입
    workflow[INPUT_NODE_ID]["inputs"]["image"] = f"fullbody/{uploaded_filename}"

    # 4) ComfyUI 서버 연결
    ws = comfyui_manager.connect_comfyui_server()
    if ws is None:
        return

    # 5) 워크플로우 실행 및 결과 추출   
    outputs = comfyui_manager.get_outputs(ws, workflow)
    ws.close()

    items = []
    # 걷기 1장
    for img in outputs.get(OUTPUT_NODE_ID, {}).get("images", []):
        b = get_image(img["filename"], img["subfolder"], img["type"])
        items.append((f"{img['filename']}", b))

    zip = image_utils.create_zip_from_images(items)
    return zip

# ==================================== 프레임 시트 워크플로우 실행 ====================================
def run_framesheet_workflow(image: UploadFile, video: UploadFile):

    # 1) 워크플로우 로드
    workflow_path = os.path.join(WF_DIR, WORKFLOW_2)
    workflow = get_workflow(workflow_path)
    if not workflow:
        return
    
    INPUT_IMAGE_ID = "12"
    INPUT_VIDEO_ID = "39"

    OUTPUT_WALK_ID = "96" # 걷기 1장
    OUPUT_RUN_ID = "97" # 뛰기 1장
    OUTPUT_DANCE_ID = "98" # 다수 프레임

    # 2) ComfyUI 서버에 파일 업로드
    #   - 사진은 input/pixel/ 밑으로
    image.file.seek(0)
    image_name = _upload_to_comfy(image, subfolder="pixel")
    #   - 비디오는 input/video/ 밑으로 (확장자 상관없이 저장되고 VHS_LoadVideo가 파일명으로 로드)
    video.file.seek(0)
    video_name = _upload_to_comfy(video, subfolder="video")

    # 3) 업로드된 파일명을 LoadImage 노드에 삽입
    workflow[INPUT_IMAGE_ID]["inputs"]["image"] = f"pixel/{image_name}"
    workflow[INPUT_VIDEO_ID]["inputs"]["video"] = f"video/{video_name}"

    # 4) ComfyUI 서버 연결
    ws = comfyui_manager.connect_comfyui_server()
    if ws is None:
        raise RuntimeError("Comfy UI WebSocket 연결 실패")


    # 5) 워크플로우 실행 및 결과 추출   
    outputs = comfyui_manager.get_outputs(ws, workflow)
    ws.close()

    items = []
    # 걷기 1장
    for img in outputs.get(OUTPUT_WALK_ID, {}).get("images", []):
        b = get_image(img["filename"], img["subfolder"], img["type"])
        items.append((f"{img['filename']}", b))
        break  # 1장만

    # 뛰기 1장
    for img in outputs.get(OUPUT_RUN_ID, {}).get("images", []):
        b = get_image(img["filename"], img["subfolder"], img["type"])
        items.append((f"{img['filename']}", b))
        break  # 1장만

    # 프레임 다수
    for img in outputs.get(OUTPUT_DANCE_ID, {}).get("images", []):
        b = get_image(img["filename"], img["subfolder"], img["type"])
        items.append((f"{img['filename']}", b))

    zip = image_utils.create_zip_from_images(items)
    return zip



# ================== 워크플로우 파일 가져오기 ==================
def get_workflow(workflow_path):
    try:
        with open(workflow_path, "r", encoding="utf-8") as f:
            workflow = json.load(f)
            
        if not workflow:
            raise Exception()
    
        logging.info("워크플로우 로딩 성공.")
        return workflow
        
    except FileNotFoundError:
        logging.error(f"파일을 찾을 수 없습니다: {workflow_path}")
    except IOError as e:
        logging.error(f"파일을 읽는 중 오류가 발생했습니다. {e}")
    except json.JSONDecodeError as e:
        logging.error(f"JSON 디코딩 오류: {e}")

    logging.warning("워크플로우 반환 실패")
    return None


