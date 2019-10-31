from . import common
from azure.cognitiveservices.vision.customvision.training import CustomVisionTrainingClient
from azure.cognitiveservices.vision.customvision.training.models import ImageFileCreateEntry

trainer = CustomVisionTrainingClient(api_key=common.custom_vision_training_key, endpoint=common.custom_vision_endpoint)

def create_images_from_files(file_name, buffer, project_id):
    images = []
    images.append(ImageFileCreateEntry(name=file_name, contents=buffer.getvalue()))
    result = trainer.create_images_from_files(project_id, images=images)
    if not result.is_batch_successful:
        return 'Image status: {0}'.format(result.images[0].status)
    else:
        return ''

def get_projects():
    return trainer.get_projects()