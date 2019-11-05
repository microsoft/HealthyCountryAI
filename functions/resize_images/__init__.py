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

    logging.info(os.getenv('CUSTOM_VISION_ANIMAL_ITERATION_NAME'))
    logging.info(os.getenv('CUSTOM_VISION_ANIMAL_PROJECT_ID'))
    logging.info(os.getenv('CUSTOM_VISION_PARRAGRASS_ITERATION_NAME'))
    logging.info(os.getenv('CUSTOM_VISION_PARRAGRASS_PROJECT_ID'))
    logging.info(os.getenv('CUSTOM_VISION_PREDICTION_KEY'))
    logging.info(os.getenv('CUSTOM_VISION_TRAINING_KEY'))
    logging.info(os.getenv('HEALTHY_HABITAT_AI_STORAGE_ACCOUNT_NAME'))
    logging.info(os.getenv('HEALTHY_HABITAT_AI_STORAGE_ACCOUNT_KEY'))

    body = req.get_json()

    if is_subscription_validation_event(body):
        return func.HttpResponse(get_response(body))
    else:
        if is_blob_created_event(body):
            result = resize_blob(body)

            if result is 'Success':
                return func.HttpResponse(status_code=200)

    return func.HttpResponse(status_code=400)

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

    azure_storage.blob_service_create_blob_from_bytes(common.healthy_habitat_storage_account_name, common.healthy_habitat_storage_account_key, common.resized_container_name, '{0}/{1}'.format(container_name, blob_name), buffer.getvalue())

    return 'Success'

def get_response(body):
    logging.info('In get_response...')
    response = {}
    response['validationResponse'] = body[0]['data']['validationCode']
    return json.dumps(response)

def is_blob_created_event(body):
    logging.info('In is_blob_created_event...')
    logging.info(body)
    logging.info(body[0]['eventType'])
    return body and body[0] and body[0]['eventType'] and body[0]['eventType'] == "Microsoft.Storage.BlobCreated"

def is_subscription_validation_event(body):
    logging.info('In is_subscription_validation_event...')
    logging.info(body)
    logging.info(body[0]['eventType'])
    return body and body[0] and body[0]['eventType'] and body[0]['eventType'] == "Microsoft.EventGrid.SubscriptionValidationEvent"