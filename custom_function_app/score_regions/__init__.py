import cv2, datetime, io, json, logging, os, rasterio, sys
import azure.functions as func
import numpy as np
from . import azure_storage
from . import common
from . import custom_vision
from . import sql_database
from azure.cognitiveservices.vision.customvision.training import CustomVisionTrainingClient
from azure.cognitiveservices.vision.customvision.training.models import ImageFileCreateEntry
from os import listdir
from PIL import Image
from rasterio.windows import Window

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

def get_raster(data_path, container_name, date_of_flight, blob_name):
    file_path = os.path.join(data_path, blob_name)

    if os.path.exists(file_path):
        os.remove(file_path)

    start = datetime.datetime.now()
    logging.info('Downloading {0} started at {1}...'.format(blob_name, start))
    azure_storage.blob_service_get_blob_to_path(common.healthy_habitat_storage_account_name, common.healthy_habitat_storage_account_key, container_name, '{0}/{1}'.format(date_of_flight, blob_name), file_path)
    stop = datetime.datetime.now()
    logging.info('{0} downloaded in {1} seconds to {2}...'.format(blob_name, (stop - start).total_seconds(), file_path))

    start = datetime.datetime.now()
    logging.info('Opening {0} started at {1}...'.format(blob_name, start))
    raster = rasterio.open(file_path)
    stop = datetime.datetime.now()
    logging.info('{0} opened in {1} seconds.'.format(blob_name, (stop - start).total_seconds()))

    return raster

def get_latest_iteration(project_id):
    iterations = custom_vision.get_iterations(project_id)
        
    if len(iterations) > 0:
        return iterations.sort(reverse=True, key=lambda iteration: iteration.last_modified)[0]
    else:
        return None

def get_projects(container_name):
    projects = custom_vision.get_projects()
    projects.sort(key=lambda project: project.name)
    return [project for project in projects if container_name in project.name]

def parse_body(body):
    url = body[0]['data']['url']
    logging.info(url)

    parts = url.split('/')

    container_name = parts[-3]
    logging.info(container_name)

    date_of_flight = parts[-2]
    logging.info(date_of_flight)

    blob_name = parts[-1]
    logging.info(blob_name)

    return url, container_name, date_of_flight, blob_name

def score_regions_from_blob(body):
    logging.info('In score_regions_from_blob...')
    url, container_name, date_of_flight, blob_name = parse_body(body)
    projects = get_projects(container_name)
    logging.info('Found Projects {}'.format(projects))

    latest_iterations = {}

    for project in projects:
        project_id = project.id
        latest_iterations[project_id] = get_latest_iteration(project_id)

    logging.info('Found Iterations {}'.format(latest_iterations))

    data_path = os.path.join(os.sep, 'home', 'data') # Using os.sep is a bit naff...

    raster = get_raster(data_path, container_name, date_of_flight, blob_name)

    raster_height = raster.height
    raster_width = raster.width

    logging.info(raster_width)
    logging.info(raster_height)
    logging.info(raster.count)

    height = 228
    width = 304

    count = 0

    for y in range(2000, raster_height, height):
        for x in range(2000, raster_width, width):
            if count < 10:
                logging.info(x)
                logging.info(y)

                region_name = '{0}_Region_{1}.JPG'.format(blob_name.split('.')[0], count)

                region_name_path = os.path.join(data_path, region_name)

                logging.info(region_name_path)

                window = raster.read(window=rasterio.windows.Window(x, y, width, height))

                profile = {
                    "driver": "JPEG",
                    "count": 4,
                    "height": height,
                    "width": width,
                    'dtype': 'uint8'
                }
                
                with rasterio.open(region_name_path, 'w', **profile) as out:
                    out.write(window)

                logging.info(listdir(data_path))

                y1 = (y + height) / 2
                x1 = (x + width) / 2
                coordinates = raster.xy(x1, y1)
                latitude = coordinates[0]
                longitude = coordinates[1]

                logging.info('{0} {1}'.format(latitude, longitude))

                region = cv2.imread(region_name_path)
                region = cv2.cvtColor(region, cv2.COLOR_BGR2RGB)

                buffer = io.BytesIO()

                Image.fromarray(region).save(buffer, format='JPEG')

                for project_id, iteration in latest_iterations.items():
                    logging.info(project_id)
                    logging.info(iteration.name)
                
                #project_id = 'd3bbda39-e52f-497b-9ed6-b3f27a63d516' # Habitat
                #iteration_name = 'ubir-kurrung-habitat-Iteration1' # Habitat

                project_id = 'd4892285-f0da-466b-9122-8c02cc370013' # Animals
                iteration_name = 'cannonhill-wurrkeng-animals-Iteration5' # Animals


                #result = custom_vision.classify_image(project_id, iteration_name, buffer)

                result = custom_vision.detect_image(project_id, iteration_name, buffer)

                predictions = result.predictions

                for prediction in predictions:
                    logging.info(prediction)

                    location_of_flight = container_name
                    season = container_name
                    label = prediction.tag_name
                    probability = prediction.probability
                    url = ''
                    bounding_box = ''

                    sql_database.insert_animal_result(date_of_flight, location_of_flight, season, region_name, label, probability, url, latitude, longitude, bounding_box, logging)

                if os.path.exists(region_name_path):
                    os.remove(region_name_path)

                count += 1

        return 'Success'
    # else:
    #     return ''

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