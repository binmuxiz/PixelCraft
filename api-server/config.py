# ====== 환경 ======

import os
from dotenv import load_dotenv
load_dotenv()

COMFY_SERVER_ADDRESS = os.getenv("COMFY_SERVER_ADDRESS")  # ComfyUI 서버 주소

APP_ROOT   = os.path.dirname(os.path.abspath(__file__))
INPUT_DIR  = os.path.join(APP_ROOT, "input-temp")
OUTPUT_DIR = os.path.join(APP_ROOT, "output-temp") 
WF_DIR     = os.path.join(APP_ROOT, "workflows")

os.makedirs(INPUT_DIR, exist_ok=True)
os.makedirs(OUTPUT_DIR, exist_ok=True)
