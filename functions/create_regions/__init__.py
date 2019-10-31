import io, json, logging, os
import azure.functions as func
import numpy as np
from . import azure_storage
from . import common
from . import custom_vision
from PIL import Image

def main(req: func.HttpRequest) -> func.HttpResponse:
    '''
    Create regions from a larger image, suitable to labelling with CustomVision.ai.
    '''
    logging.info('Create Regions Function received a request.')

    body = req.get_json()

    if is_subscription_validation_event(body):
        return func.HttpResponse(get_response(body))
    else:
        if is_blob_created_event(body):
            result = create_regions_from_blob(body)

            if result is 'Success':
                return func.HttpResponse(status_code=200)

    return func.HttpResponse(status_code=400)

def create_regions_from_blob(body):
    url = body[0]['data']['url']
    container_name = url.split('/', 4)[-2]
    blob_name = url.split('/', 4)[-1]

    projects = custom_vision.get_projects()

    project_id = [project.id for project in projects if project.name == container_name][0]

    if project_id:
        blob = azure_storage.blob_service_get_blob_to_bytes(common.healthy_habitat_storage_account_name, common.healthy_habitat_storage_account_key, container_name, blob_name)
        image = np.array(Image.open(io.BytesIO(blob.content)))
        image_shape = image.shape
        height = 228
        width = 304
        count = 0

        for y in range(0, image_shape[0], height):
            for x in range(0, image_shape[1], width):
                region = image[y:y + height, x:x + width]
                file_name = '{0}_Region_{1}.jpg'.format(blob_name.split('.')[0], count)
                buffer = io.BytesIO()
                Image.fromarray(region).save(buffer, format='JPEG')
                result = custom_vision.create_images_from_files(file_name, buffer, project_id)
                if result is not '':
                    logging.error(result)

        return 'Success'
    else:
        logging.error('Create a CustomVision.ai Project matching the container name: {0}'.format(container_name))
        return ''

def get_response(body):
    response = {}
    response['validationResponse'] = body[0]['data']['validationCode']
    return json.dumps(response)

def is_blob_created_event(body):
    return body and body[0] and body[0]['eventType'] and body[0]['eventType'] == "Microsoft.Storage.BlobCreated"

def is_subscription_validation_event(body):
    return body and body[0] and body[0]['eventType'] and body[0]['eventType'] == "Microsoft.EventGrid.SubscriptionValidationEvent"