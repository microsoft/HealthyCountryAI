import io, json, logging, os
import azure.functions as func
import numpy as np
from . import azure_storage
from . import common
from . import custom_vision
from PIL import Image
from . import sql_database

def main(req: func.HttpRequest) -> func.HttpResponse:
    '''
    Resize an image.
    '''
    logging.info('Resize Image Function received a request.')

    body = req.get_json()
    logging.info(body)

    result = resize_blob(body)

    return func.HttpResponse(result, status_code=200)

def resize_blob(body):
    logging.info('In resize_blob...')

    url = body[0]['data']['url']
    logging.info(url)

    container_name = url.split('/', 4)[-2]
    logging.info(container_name)

    blob_name = url.split('/', 4)[-1]
    logging.info(blob_name)

    height = int(common.image_resize_height) 
    width = int(common.image_resize_width)

    blob = azure_storage.blob_service_get_blob_to_bytes(common.healthy_habitat_storage_account_name, common.healthy_habitat_storage_account_key, container_name, blob_name)

    image = Image.open(io.BytesIO(blob.content))
    logging.info(image.size)

    image = image.resize((width, height))
    logging.info(image.size)

    buffer = io.BytesIO()
    image.save(buffer, format='JPEG')

    path = '{0}/{1}'.format(container_name, blob_name)

    azure_storage.blob_service_create_blob_from_bytes(common.healthy_habitat_storage_account_name, common.healthy_habitat_storage_account_key, common.resized_container_name, path, buffer.getvalue())

    sas_url = azure_storage.blob_service_generate_blob_shared_access_signature(common.healthy_habitat_storage_account_name, common.healthy_habitat_storage_account_key, container_name, path)
    logging.info(sas_url)

    return sas_url