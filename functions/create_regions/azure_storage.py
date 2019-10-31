from azure.common import AzureMissingResourceHttpError
from azure.storage.blob import BlockBlobService, PublicAccess
from azure.storage.file import FileService
from azure.storage.table import TableService, Entity

# Blob Service...
def get_block_blob_service(account_name, storage_key):
    return BlockBlobService(account_name=account_name, account_key=storage_key)

def blob_service_create_container(account_name, storage_key, container_name):
    containers = blob_service_list_containers(account_name, storage_key)
    if container_name not in containers:
        block_blob_service = get_block_blob_service(account_name, storage_key)
        block_blob_service.create_container(container_name)
        block_blob_service.set_container_acl(container_name, public_access=PublicAccess.Container)

def blob_service_create_blob_from_bytes(account_name, storage_key, container_name, blob_name, blob):
    block_blob_service = get_block_blob_service(account_name, storage_key)
    block_blob_service.create_blob_from_bytes(container_name, blob_name, blob)

def blob_service_get_blob_to_bytes(account_name, storage_key, container_name, blob_name):
    block_blob_service = get_block_blob_service(account_name, storage_key)
    block_blob_service.get_blob_to_bytes(container_name, blob_name)

def blob_service_get_blob_to_path(account_name, storage_key, container_name, blob_name, file_path):
    block_blob_service = get_block_blob_service(account_name, storage_key)
    block_blob_service.get_blob_to_path(container_name, blob_name, file_path)

def blob_service_insert(account_name, storage_key, container_name, blob_name, text):
    block_blob_service = get_block_blob_service(account_name, storage_key)
    block_blob_service.create_blob_from_text(container_name, blob_name, text)

def blob_service_list_blobs(account_name, storage_key, container_name):
    blobs = []
    block_blob_service = get_block_blob_service(account_name, storage_key)
    generator = block_blob_service.list_blobs(container_name)
    for blob in generator:
        blobs.append(blob.name)
    return blobs

def blob_service_list_containers(account_name, storage_key):
    containers = []
    block_blob_service = get_block_blob_service(account_name, storage_key)
    generator = block_blob_service.list_containers()
    for container in generator:
        containers.append(container.name)
    return containers

# File Service...
def get_file_service(account_name, storage_key):
    return FileService(account_name=account_name, account_key=storage_key)

def file_service_list_directories_and_files(account_name, storage_key, share_name, directory_name):
    file_or_dirs = []
    file_service = get_file_service(account_name, storage_key)
    generator = file_service.list_directories_and_files(share_name, directory_name)
    for file_or_dir in generator:
        file_or_dirs.append(file_or_dir.name)
    return file_or_dirs

# Table Service...
def get_table_service(account_name, storage_key):
    return TableService(account_name=account_name, account_key=storage_key)

def table_service_get_entity(account_name, storage_key, table, partition_key, row_key):
    table_service = get_table_service(account_name, storage_key)
    return table_service.get_entity(table, partition_key, row_key)

def table_service_insert(account_name, storage_key, table, entity):
    table_service = get_table_service(account_name, storage_key)
    table_service.insert_entity(table, entity)
    
def table_service_query_entities(account_name, storage_key, table, filter):
    table_service = get_table_service(account_name, storage_key)
    return table_service.query_entities(table, filter)