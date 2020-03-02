import argparse, cv2, imutils, os
import numpy as np
from imutils import paths

def main(req: func.HttpRequest) -> func.HttpResponse:
    
    logging.info('Create stitch Function received a request.')

    body = req.get_json()

    if is_subscription_validation_event(body):
        return func.HttpResponse(get_response(body))
    else:
        if is_blob_created_event(body):
            result = create_stitch(body)

            if result is 'Success':
                return func.HttpResponse(status_code=200)

    return func.HttpResponse(status_code=400)

def create_stitich(body):
    in_path = os.path.join('..', 'data', 'interim', 'frames', '2019-08-13-1700') # 4Dec18 Ubir Geese
    out_path = os.path.join('..', 'data', 'interim', 'stitched')

    image_paths = sorted(list(paths.list_images(in_path)))
    print(image_paths)

    images = []

    for image_path in image_paths:
        #if 'DJI_0455' in image_path or 'DJI_0456' in image_path:
        image = cv2.imread(image_path)
        images.append(image)

    stitcher = cv2.createStitcher() if imutils.is_cv3() else cv2.Stitcher_create()
    (status, stitched) = stitcher.stitch(images)
    file_name = '{0}.jpg'.format(in_path.split(os.sep)[-1])

    out_path = os.path.join(out_path, file_name)

    if status == 0:
        cv2.imwrite(out_path, stitched)

        cv2.imshow("Stitched", stitched)
        cv2.waitKey(0)
    else:
        #print("[INFO] image stitching failed ({})".format(status))

def get_response(body):
    logging.info('In get_response...')
    response = {}
    response['validationResponse'] = body[0]['data']['validationCode']
    return json.dumps(response)

def is_blob_created_event(body):
    logging.info('In is_stitch_created_event...')
    logging.info(body)
    logging.info(body[0]['eventType'])
    return body and body[0] and body[0]['eventType'] and body[0]['eventType'] == "Microsoft.Storage.BlobCreated"

def is_subscription_validation_event(body):
    logging.info('In is_subscription_validation_event...')
    logging.info(body)
    logging.info(body[0]['eventType'])
    return body and body[0] and body[0]['eventType'] and body[0]['eventType'] == "Microsoft.EventGrid.SubscriptionValidationEvent"