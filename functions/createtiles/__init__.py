import logging
import json
import azure.functions as func
from . import create_tiles

def main(req: func.HttpRequest) -> func.HttpResponse:
    # This will output to postman
    logging.info('Python HTTP trigger function processed a request.')
    req_body = req.get_json()

    if is_validation_event(req_body):
        return func.HttpResponse(validate_eg(req_body))

    elif is_blob_created_event(req_body):
        result = create_tiles.load(req_body)

        if result is "Success":
            return func.HttpResponse("Successfully uploaded data",status_code=200)
        else:
            return func.HttpResponse("Bad Request", status_code=400)

    else: 
        pass

# Check for validation event from event grid
def is_validation_event(req_body):
    return req_body and req_body[0] and req_body[0]['eventType'] and req_body[0]['eventType'] == "Microsoft.EventGrid.SubscriptionValidationEvent"

# If blob created event, then true
def is_blob_created_event(req_body):
    return req_body and req_body[0] and req_body[0]['eventType'] and req_body[0]['eventType'] == "Microsoft.Storage.BlobCreated"

# Respond to event grid webhook validation event
def validate_eg(req_body):
    result = {}
    result['validationResponse'] = req_body[0]['data']['validationCode']
    return json.dumps(result)