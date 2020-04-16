import datetime, io, json, logging, os, rasterio, sys
import azure.functions as func
import numpy as np
from . import azure_storage
from . import common
from . import custom_vision
from azure.cognitiveservices.vision.customvision.training import CustomVisionTrainingClient
from azure.cognitiveservices.vision.customvision.training.models import ImageFileCreateEntry
from PIL import Image

def main(req: func.HttpRequest) -> func.HttpResponse:
    '''
    Score regions from a larger TIFF.
    '''
    logging.info('Score Regions Function received a request.')

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

    parts = url.split('/')

    container_name = parts[-3]
    logging.info(container_name)

    date_of_flight = parts[-2]
    logging.info(date_of_flight)

    blob_name = parts[-1]
    logging.info(blob_name)

    file_path = os.path.join(os.sep, 'home', 'data', blob_name) # Using os.sep is a bit naff...

    if os.path.exists(file_path):
        os.remove(file_path)

    start = datetime.datetime.now()
    logging.info('Downloading {0} started at {1}...'.format(blob_name, start))
    azure_storage.blob_service_get_blob_to_path(common.healthy_habitat_storage_account_name, common.healthy_habitat_storage_account_key, container_name, '{0}/{1}'.format(date_of_flight, blob_name), file_path)
    stop = datetime.datetime.now()
    logging.info('{0} downloaded in {1} seconds to {2}...'.format(blob_name, (stop - start).total_seconds(), file_path))

    start = datetime.datetime.now()
    logging.info('Opening {0} started at {1}...'.format(blob_name, start))
    dataset = rasterio.open(file_path)
    stop = datetime.datetime.now()
    logging.info('{0} opened in {1} seconds.'.format(blob_name, (stop - start).total_seconds()))

    projects = custom_vision.get_projects()

    project_ids = [project.id for project in projects if container_name in project.name]

    logging.info('Found Project Ids {}'.format(project_ids))

    if len(project_ids) > 0:
        for project_id in project_ids:
            logging.info('project_id {}'.format(project_id))

            iterations = custom_vision.get_iterations(project_id)
            
            if len(iterations) > 0:
                iterations.sort(reverse=True, key=lambda iteration: iteration.last_modified)

                latest_iteration = iterations[0]

                logging.info('latest_iteration {}'.format(latest_iteration.publish_name))

        return 'Success'
    else:
        logging.error('No CustomVision.ai Projects found containing: {0}'.format(container_name))
        return ''

def get_response(body):
    logging.info('In get_response...')
    response = {}
    response['validationResponse'] = body[0]['data']['validationCode']
    return json.dumps(response)

def is_blob_created_event(body):
    logging.info('In is_blob_created_event...')
    return body and body[0] and body[0]['eventType'] and body[0]['eventType'] == "Microsoft.Storage.BlobCreated"

def is_subscription_validation_event(body):
    logging.info('In is_subscription_validation_event...')
    return body and body[0] and body[0]['eventType'] and body[0]['eventType'] == "Microsoft.EventGrid.SubscriptionValidationEvent"

'''
import logging

import azure.functions as func


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    name = req.params.get('name')
    if not name:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            name = req_body.get('name')

    if name:
        return func.HttpResponse(f"Hello {name}!")
    else:
        return func.HttpResponse(
             "Please pass a name on the query string or in the request body",
             status_code=400
        )
'''