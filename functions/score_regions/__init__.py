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
    Create regions from a larger image, score with CustomVision.ai, and save to Storage.
    '''
    logging.info('Score Regions Function received a request.')

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
            result = score_regions_from_blob(body)

            if result is 'Success':
                return func.HttpResponse(status_code=200)

    return func.HttpResponse(status_code=400)

def score_regions_from_blob(body):
    logging.info('In score_regions_from_blob...')

    url = body[0]['data']['url']
    logging.info(url)

    container_name = url.split('/', 4)[-2]
    logging.info(container_name)

    location_of_flight = container_name.split('-')[0]
    logging.info(location_of_flight)

    season = container_name.split('-')[1]
    logging.info(season)

    model_type = url.split('/')[-3]
    logging.info(model_type)

    date_of_flight = url.split('/')[-2]
    logging.info(date_of_flight)

    project_name = '{0}-{1}'.format(container_name, model_type)
    logging.info(project_name)

    blob_name = url.split('/', 4)[-1]
    logging.info(blob_name)

    projects = custom_vision.get_projects()

    project_ids = [project.id for project in projects if project.name == project_name]

    print(project_ids)

    for project in projects:
        logging.info(project.id)
        logging.info(project.name)

    if len(project_ids) == 1:
        project_id = project_ids[0]
        logging.info(project_id)

        iterations = custom_vision.get_iterations(project_id)

        logging.info(len(iterations))
        logging.info(iterations)

        for iteration in iterations:
            logging.info(iteration.id)
            logging.info(iteration.name)

        '''
        Having got to here unpacking the variables above to work out which CustomVision.ai Project/Iteration to call...for the moment
        instead, we're going to hard code (0_0) the Project Id and Iteration Id :(
        '''
        iteration_name = ''

        if model_type == 'animals':
            iteration_name = common.custom_vision_animal_iteration_name
            project_id = common.custom_vision_animal_project_id
        elif model_type == 'parragrass':
            iteration_name = common.custom_vision_parragrass_iteration_name
            project_id = common.custom_vision_parragrass_project_id

        logging.info(model_type)
        logging.info(iteration_name)
        logging.info(project_id)

        if iteration_name == '':
            logging.error('Set environment variables for CUSTOM_VISION_ANIMAL_ITERATION_NAME and CUSTOM_VISION_PARRAGRASS_ITERATION_NAME')
            return ''

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
                logging.info('Scoring {0}...'.format(region_name))

                result = custom_vision.detect_image(project_id, iteration_name, buffer)

                logging.info(result)

                for prediction in result.predictions:
                    logging.info(prediction.bounding_box)
                    logging.info(prediction.tag_id)
                    logging.info(prediction.tag_name)
                    logging.info(prediction.probability)
                    sql_database.insert_animal_result(date_of_flight, location_of_flight, season, blob_name, region_name, prediction.tag_name, prediction.probability, '')

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