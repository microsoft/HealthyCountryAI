import logging, pyodbc
from . import common

server = common.sql_server
logging.info(server)
database = common.sql_database
logging.info(database)
username = common.sql_database_username
logging.info(username)
password = common.sql_database_password
logging.info(password)
driver= '{ODBC Driver 17 for SQL Server}'

def insert_animal_result(date_of_flight, location_of_flight, season, image_name, region_name, label, probability, url, logging):
    statement = 'INSERT INTO dbo.Animals (DateOfFlight, LocationOfFlight, Season, ImageName, RegionName, Label, Probability, URL) VALUES (\'{0}\', \'{1}\', \'{2}\', \'{3}\', \'{4}\', \'{5}\', {6}, \'{7}\')'.format(date_of_flight, location_of_flight, season, image_name, region_name, label, probability, url)
    logging.info(statement)
    execute(statement)

def insert_paragrass_result(date_of_flight, location_of_flight, season, image_name, region_name, label, probability, url, logging):
    statement = 'INSERT INTO dbo.Paragrass (DateOfFlight, LocationOfFlight, Season, ImageName, RegionName, Label, Probability, URL) VALUES (\'{0}\', \'{1}\', \'{2}\', \'{3}\', \'{4}\', \'{5}\', {6}, \'{7}\')'.format(date_of_flight, location_of_flight, season, image_name, region_name, label, probability, url)
    logging.info(statement)
    execute(statement)
    
def execute(statement):
    cnxn = pyodbc.connect('DRIVER='+driver+';SERVER='+server+';PORT=1433;DATABASE='+database+';UID='+username+';PWD='+ password)
    cursor = cnxn.cursor()
    cursor.execute(statement) 
    cnxn.commit()