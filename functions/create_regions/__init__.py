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
    logging.info('In create_regions_from_blob...')

    url = body[0]['data']['url']
    logging.info(url)

    container_name = url.split('/', 4)[-2]
    logging.info(container_name)

    model_type = url.split('/')[-3]
    logging.info(model_type)

    project_name = '{0}-{1}'.format(container_name, model_type)
    logging.info(project_name)

    blob_name = url.split('/', 4)[-1]
    logging.info(blob_name)

    projects = custom_vision.get_projects()

    project_ids = [project.id for project in projects if project.name == project_name]

    if len(project_ids) == 1:
        project_id = project_ids[0]
        logging.info(project_id)

        blob = azure_storage.blob_service_get_blob_to_bytes(common.healthy_habitat_storage_account_name, common.healthy_habitat_storage_account_key, container_name, blob_name)

        image = np.array(Image.open(io.BytesIO(blob.content)))
        image_shape = image.shape

        height = 228
        width = 304

        count = 0

        for y in range(0, image_shape[0], height):
            for x in range(0, image_shape[1], width):
                region = image[y:y + height, x:x + width]

                buffer = io.BytesIO()

                Image.fromarray(region).save(buffer, format='JPEG')

                region_name = '{0}_Region_{1}.jpg'.format(blob_name.split('.')[0], count)
                logging.info('Creating {0}...'.format(region_name))

                result = custom_vision.create_images_from_files(region_name, buffer, project_id)

                if result is not '':
                    logging.error(result)
                else:
                    count += 1
        
        logging.info('Created {0}.'.format(count))

        return 'Success'
    else:
        logging.error('Create one CustomVision.ai Project matching the project name: {0}'.format(project_name))
        return ''

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