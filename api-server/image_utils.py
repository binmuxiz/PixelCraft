import os
import io
from io import BytesIO
from PIL import Image
from datetime import datetime
import logging
import zipfile
from typing import List, Tuple, Optional
import aiofiles
from fastapi import UploadFile


# 로그 설정
logging.basicConfig(
    level=logging.DEBUG,  # INFO, WARNING, ERROR, DEBUG 중 선택 가능
    format="%(asctime)s [%(levelname)s] %(message)s",
    handlers=[
        logging.FileHandler("./logging/image_utils_log.log", encoding="utf-8"),  # 로그 파일에 기록
        logging.StreamHandler()  # 콘솔에도 출력
    ]
)

# ==================================== 이미지 경로를 받아 zip 파일로 압축 ====================================

def create_zip_from_images(image_data_list: List[Tuple[str, bytes]]) -> Optional[BytesIO]:
    """
    메모리 상의 이미지 데이터들을 ZIP으로 압축합니다. (디스크 저장 안 함)

    Args:
        image_data_list: (filename, image_bytes) 튜플 리스트

    Returns:
        BytesIO: 메모리에 생성된 ZIP 파일 (seek(0) 상태)
    """
    try:
        zip = BytesIO()

        with zipfile.ZipFile(zip, 'w', zipfile.ZIP_DEFLATED) as zipf:
            for filename, img_data in image_data_list:
                if not img_data:
                    logging.warning(f"⚠️ 비어있는 이미지 데이터: {filename}")
                    continue
                # write() 대신 writestr로 바로 메모리 데이터를 ZIP에 추가
                zipf.writestr(filename, img_data)

        zip.seek(0)
        return zip

    except Exception as e:
        logging.error(f"❌ ZIP 파일 생성 중 오류 발생: {str(e)}", exc_info=True)
        return None
    
    
    


async def save_upload_image(file: UploadFile, destination: str) -> bool:
    try:
        dir_name = os.path.dirname(destination)
        if not os.path.exists(dir_name):
            logging.warning(f"📂 폴더가 존재하지 않습니다: {dir_name}")
    
        logging.debug(f"📄 파일 저장 시작: {destination}")

        async with aiofiles.open(destination, "wb") as outfile:
            while content := await file.read(1024):
                await outfile.write(content)

        logging.debug(f"✅ 파일 저장 성공: {destination}")

        return True
    
    except Exception as e:
        logging.error(e, exc_info=True)
    
    return False



    
def save_image(output_dir: str, image_data: bytes, prefix: str = "") -> Optional[str]:
    """
    ComfyUI에서 받은 이미지 데이터를 저장하고 저장된 경로를 반환합니다.
    
    Args:
        output_dir: 이미지를 저장할 디렉토리 경로
        image_data: ComfyUI에서 반환한 이미지  데이터
        prefix: 파일명 앞에 붙일 접두사 (예: 'walk', 'run' 등)
        
    Returns:
        저장된 이미지의 경로 또는 실패 시 None
    """
    try:
        # 출력 디렉토리가 없으면 생성
        os.makedirs(output_dir, exist_ok=True)
        
        # 이미지 데이터 확인
        if not image_data:
            logging.error("❌ 유효한 이미지 데이터가 없습니다.")
            return None
        
        # 현재 시간을 파일명에 포함
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        
        # 파일명에 prefix 추가
        filename = f"{prefix}_{timestamp}.png"
        
        filepath = os.path.join(output_dir, filename)
        
        # 이미지 파일에 저장
        with open(filepath, "wb") as f:
            f.write(image_data)
            
        logging.info(f"✅ 이미지 저장 완료: {filepath}")
        return filepath
        
    except Exception as e:
        logging.error(f"❌ 이미지 저장 중 오류 발생: {str(e)}", exc_info=True)
        return None






# ================== 이미지 저장,압축 및 반환  ==================
def save_and_compress_imges(destination, target_images: List[bytes]) -> Optional[List[Image.Image]]:

    try:
        if target_images:
            # 메모리 안에서 ZIP 파일 만들기
            zip_buffer = BytesIO()

            with zipfile.ZipFile(zip_buffer, "w") as zip_file:
                for idx, image_data in enumerate(target_images):
                        image = Image.open(io.BytesIO(image_data))

                        timestamp = datetime.now().strftime('%Y%m%d%H%M%S')
                        file_name = f"{timestamp}-{idx}.png"

                        # 파일 저장 
                        file_path = os.path.join(destination, file_name)
                        image.save(file_path)

                        # 결과를 메모리에 저장
                        img_byte_arr = BytesIO()
                        image.save(img_byte_arr, format="PNG")
                        img_byte_arr.seek(0)

                        # zip파일에 추가
                        zip_file.writestr(file_name, img_byte_arr.read())

            zip_buffer.seek(0)
            return zip_buffer
        
    except Exception as e:
        logging.error("❌ 이미지  저장/압축 중 에러 발생", exc_info=True)
    
    return None




