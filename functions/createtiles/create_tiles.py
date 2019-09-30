import logging
import os
from azure.storage.blob import BlockBlobService
import io
import numpy as np
from PIL import Image
from azure.cognitiveservices.vision.customvision.training import CustomVisionTrainingClient
from azure.cognitiveservices.vision.customvision.training.models import ImageFileCreateEntry

### obtain application details from environment
blob_account_name = os.getenv("BLOB_ACCOUNT_NAME")
blob_account_key = os.getenv("BLOB_ACCOUNT_KEY")
block_blob_service = BlockBlobService(account_name=blob_account_name,
                                      account_key=blob_account_key)
custom_vision_training_key=os.getenv('CUSTOM_VISION_TRAINING_KEY')
ENDPOINT = os.getenv('CUSTOM_VISION_ENDPOINT')

# Get Custom Vision project
trainer = CustomVisionTrainingClient(custom_vision_training_key, endpoint=ENDPOINT)
projects = trainer.get_projects()

# ### Custom Vision
def load(req_body):
    blob_obj,filename, container_name = extract_blob_props(req_body[0]['data']['url']  )
    id = [project.id for project in projects if project.name == container_name][0]
    if id:
        image = Image.open(io.BytesIO(blob_obj.content))
        result = load_blob(image,filename, id)
    else:
        logging.error('Unable to find project matching Storage Container name: {0}'.format(container_name))  
    return result

# Extract blob container name and blob file
def extract_blob_props(url):
    blob_file_name = url.split('/',4)[-1]
    in_container_name = url.split('/',4)[-2]
    # remove file extension from blob name
    readblob = block_blob_service.get_blob_to_bytes(in_container_name,blob_file_name)                       
    return readblob, blob_file_name, in_container_name

def load_blob(image_obj, blob_file_name, project_id):
    image = np.array(image_obj)
    image_shape = image.shape
    height = 228
    width = 304
    count = 0
    
    for y in range(0, image_shape[0], height):
        for x in range(0, image_shape[1], width):
            region = image[y:y + height, x:x + width]
            file_name = '{0}_Region_{1}.jpg'.format(blob_file_name.split('.')[0], count)
            buffer = io.BytesIO()
            Image.fromarray(region).save(buffer, format='JPEG')        
            images = []
            images.append(ImageFileCreateEntry(name=file_name, contents=buffer.getvalue()))
            result = trainer.create_images_from_files(project_id, images=images)
            if not result.is_batch_successful:
                for image_create_result in result.images:
                    print("Image status: ", image_create_result.status)
            count += 1
    return "Success"