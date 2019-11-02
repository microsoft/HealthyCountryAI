import io
from . import common
from azure.cognitiveservices.vision.customvision.training import CustomVisionTrainingClient
from azure.cognitiveservices.vision.customvision.training.models import ImageFileCreateEntry
from msrest.exceptions import HttpOperationError

trainer = CustomVisionTrainingClient(api_key=common.custom_vision_training_key, endpoint=common.custom_vision_endpoint)

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

def get_projects():
    return trainer.get_projects()