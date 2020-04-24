import cv2, datetime, io, json, logging, os, rasterio, sys
import azure.functions as func
import numpy as np
from . import azure_storage
from . import common
from . import custom_vision
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

def get_latest_iteration_ids(container_name):
    projects = custom_vision.get_projects()

    projects.sort(key=lambda project: project.name)

    project_ids = [(project.id, project.name) for project in projects if container_name in project.name]

    logging.info('Found Project Ids {}'.format(project_ids))

    latest_iteration_ids = []

    for project_id in project_ids:
        iterations = custom_vision.get_iterations(project_id[0])
        
        if len(iterations) > 0:
            iterations.sort(reverse=True, key=lambda iteration: iteration.last_modified)

            latest_iteration_ids.append(iterations[0])

    logging.info('Found Iteration Ids'.format(latest_iteration_ids))

    return latest_iteration_ids

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
    latest_iteration_ids = get_latest_iteration_ids(container_name)

    data_path = os.path.join(os.sep, 'home', 'data') # Using os.sep is a bit naff...

    #if len(latest_iteration_ids) == 2:
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
                
                project_id = 'd3bbda39-e52f-497b-9ed6-b3f27a63d516'
                iteration_name = 'ubir-kurrung-habitat-Iteration1'

                #logging.info('Creating {0} in {1}...'.format(region_name, project_id))

                #result = custom_vision.create_images_from_files(region_name, buffer, project_id)

                #logging.info(result)

                result = custom_vision.detect_image(project_id, iteration_name, buffer)

                logging.info(result)

                if os.path.exists(region_name_path):
                    os.remove(region_name_path)

                count += 1

            '''
            if result_animal!=-1:
                logging.info(result)

                xc1 = 0  # egret
                xc2 = 0  # goose
                xc3 = 0  # duck
                update_indicator = 0
                for prediction in result_animal.predictions:

                    if prediction.tag_name == "duck":
                        xc1 = xc1 + 1
                    elif prediction.tag_name == "goose":
                        xc2 = xc2 + 1
                    elif prediction.tag_name == "egret":
                        xc3 = xc3 + 1

                    logging.info(prediction.tag_id)
                    logging.info(prediction.tag_name)
                    logging.info(prediction.probability)

                    logging.info(prediction.bounding_box.left)
                    logging.info(prediction.bounding_box.top)
                    logging.info(prediction.bounding_box.width)
                    logging.info(prediction.bounding_box.height)
                    boundingBox1 = '{0},{1},{2},{3}'.format(prediction.bounding_box.left, prediction.bounding_box.top,prediction.bounding_box.width,prediction.bounding_box.height)
                    logging.info(datecreated)
                    sql_database.insert_animal_result(date_of_flight, location_of_flight, season, blob_namez, sas_url,
                                                    region_namez, prediction.tag_name, prediction.probability,
                                                    boundingBox1, stitchedurl, latitude, longitude, iteration_name_animal,logging)
                    update_indicator = update_indicator + 1

                tac1 = "Goose" + ":" + str(xc2)
                tac2 = "Duck" + ":" + str(xc1)
                tac3 = "Egret" + ":" + str(xc3)

                sas_url = resize_save_image(pil_im, region_name, url)

                if update_indicator > 0:
                    sql_database.update_animal_result(region_namez, sas_url, logging)
                logging.info(sas_url)
            else:
                #save_jpg(pil_im, region_name, url)
                untagged_images_animal=custom_vision.prepare_untagged_images(buffer,region_name,untagged_images_animal)
                counter_animal = counter_animal +1
                sas_url = resize_save_image(pil_im, region_name, url)

            result_habitat = custom_vision.classify_image(project_id_habitat, iteration_name_habitat, buffer)

            if result_habitat != -1:
                logging.info(result)
                update_indicator = 0
                for prediction in result_habitat.predictions:
                    logging.info(prediction.tag_id)
                    logging.info(prediction.tag_name)
                    logging.info(prediction.probability)
                    sql_database.insert_parragrass_result(date_of_flight, location_of_flight, season, blob_namez, sas_url,
                                                        region_namez, prediction.tag_name, prediction.probability,
                                                        stitchedurl, latitude, longitude, iteration_name_habitat,logging)
                    update_indicator = update_indicator + 1

                sas_url = resize_save_image(pil_im, region_name, url)
                if update_indicator > 0:
                    sql_database.update_Paragrass_result(region_namez, sas_url, logging)
                logging.info(sas_url)
            else:
                untagged_images_habitat=custom_vision.prepare_untagged_images(buffer,region_name, untagged_images_habitat)
                counter_habitat = counter_habitat + 1

            if counter_animal >= 10:
                result_region = custom_vision.create_images_from_files(untagged_images_animal, project_id_animal)
                untagged_images_animal = []
                counter_animal = 0
            if counter_habitat >= 10:
                result_region = custom_vision.create_images_from_files(untagged_images_habitat, project_id_habitat)
                untagged_images_habitat = []
                counter_habitat=0
            '''

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