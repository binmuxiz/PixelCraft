from fastapi import FastAPI, UploadFile, File
from fastapi import HTTPException
from fastapi.responses import StreamingResponse
from fastapi.middleware.cors import CORSMiddleware
import logging

import workflow_api

# ====== logging =====
logging.basicConfig(
    level=logging.DEBUG,
    format="%(asctime)s [%(levelname)s] %(message)s",
    handlers=[
        logging.FileHandler("./logging/fastapi_server_log.log", encoding="utf-8"),
        logging.StreamHandler()
    ]
)

# ====== 앱 ======
app = FastAPI()
app.add_middleware(
    CORSMiddleware, allow_origins=["*"], allow_methods=["*"], allow_headers=["*"]
)


# ===================================== 픽셀 캐릭터 생성 요청 =====================================
@app.post("/generate_pixel_character", response_class=StreamingResponse)
async def generate_pixel_character(image: UploadFile):
    logging.debug("=== 픽셀 캐릭터 생성 요청 ====")
    
    try:
        zip = workflow_api.run_pixel_character_workflow(image)
        
        if zip:
            return StreamingResponse(
                zip,
                media_type="application/zip",
                headers={
                    "Content-Disposition": "attachment; filename=pixel_characters.zip"
                }
            )
        raise HTTPException(status_code=500, detail="픽셀 캐릭터 ZIP 생성 실패")
    
    except Exception as e:
        logging.debug(f"에러 확인 : {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"픽셀 캐릭터 생성 실패")
    


# ===================================== 프레임 시트 생성 요청 (사진, 비디오 1개씩 받음)=====================================
@app.post("/generate_framesheet", response_class=StreamingResponse)
async def generate_framesheet(image: UploadFile, video: UploadFile):
    logging.debug("=== 프레임시트 생성 요청 ===")

    try:
        zip = workflow_api.run_framesheet_workflow(image, video)
        if zip:
            return StreamingResponse(
                zip,
                media_type="application/zip",
                headers={
                    "Content-Disposition": "attachment; filename=framesheet.zip"
                }
            )
        raise HTTPException(status_code=500, detail="프레임 시트 ZIP 생성 실패")

    except Exception as e:
        logging.debug(f"에러 확인 : {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"프레임 시트 생성 실패")
    


