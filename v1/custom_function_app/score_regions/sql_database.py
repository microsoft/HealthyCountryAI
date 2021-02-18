import logging, pyodbc
from . import common

server = common.sql_server
database = common.sql_database
username = common.sql_database_username
password = common.sql_database_password

driver= '{ODBC Driver 17 for SQL Server}'

def insert_animal_result(date_of_flight, location_of_flight, season, region_name, label, probability, blob_url, latitude, longitude, logging):
    statement = 'INSERT INTO dbo.Animals (DateOfFlight, LocationOfFlight, Season, RegionName, Label, Probability, URL, Latitude, Longitude) VALUES (\'{0}\', \'{1}\', \'{2}\', \'{3}\', \'{4}\', \'{5}\', \'{6}\', \'{7}\', \'{8}\')'.format(date_of_flight, location_of_flight, season, region_name, label, probability, blob_url, latitude, longitude)
    logging.info(statement)
    execute(statement, logging)

def insert_habitat_result(date_of_flight, location_of_flight, season, region_name, label, probability, blob_url, latitude, longitude, logging):
    statement = 'INSERT INTO dbo.Habitat (DateOfFlight, LocationOfFlight, Season, RegionName, Label, Probability, URL, Latitude, Longitude) VALUES (\'{0}\', \'{1}\', \'{2}\', \'{3}\', \'{4}\', \'{5}\', \'{6}\', \'{7}\', \'{8}\')'.format(date_of_flight, location_of_flight, season, region_name, label, probability, blob_url, latitude, longitude)
    logging.info(statement)
    execute(statement, logging)
    
def execute(statement, logging):
    cnxn = pyodbc.connect('DRIVER='+driver+';SERVER='+server+';PORT=1433;DATABASE='+database+';UID='+username+';PWD='+ password)
    cursor = cnxn.cursor()
    cursor.execute(statement) 
    cnxn.commit()