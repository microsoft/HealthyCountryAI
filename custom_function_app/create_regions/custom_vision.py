import io
from . import common
from azure.cognitiveservices.vision.customvision.prediction import CustomVisionPredictionClient
from azure.cognitiveservices.vision.customvision.training import CustomVisionTrainingClient
from azure.cognitiveservices.vision.customvision.training.models import ImageFileCreateEntry
from msrest.exceptions import HttpOperationError

#predictor = CustomVisionPredictionClient(api_key=common.custom_vision_prediction_key, endpoint=common.custom_vision_endpoint)
trainer = CustomVisionTrainingClient(api_key=common.custom_vision_training_key, endpoint=common.custom_vision_endpoint)

'''
def classify_image(project_id, iteration_name, buffer):
    return predictor.classify_image(project_id, iteration_name, buffer.getvalue())
'''

def create_images_from_files(name, buffer, project_id):
    images = []
    images.append(ImageFileCreateEntry(name=name, contents=buffer.getvalue()))
    try:
        result = trainer.create_images_from_files(project_id, images=images)

        if not result.is_batch_successful:
            return 'Image status: {0}'.format(result.images[0].status)
        else:
            return ''
    except HttpOperationError as e:
        return e.response.text

'''
def detect_image(project_id, iteration_name, buffer):
    return predictor.detect_image(project_id, iteration_name, buffer.getvalue())
'''

def get_iterations(project_id):
    return trainer.get_iterations(project_id)

def get_projects():
    return trainer.get_projects()