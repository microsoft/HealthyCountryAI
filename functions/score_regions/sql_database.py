import logging, pyodbc
from . import common

server = 'healthyhabitatai.database.windows.net'
database = 'HealthyHabitatAI'
username = 'svanbodegraven'
password = common.sql_database_password
driver= '{ODBC Driver 17 for SQL Server}'

def insert_animal_result(date_of_flight, location_of_flight, season, image_name, region_name, label, probability, logging):
    statement = 'INSERT INTO dbo.Animals (DateOfFlight, LocationOfFlight, Season, ImageName, RegionName, Label, Probability) VALUES (\'{0}\', \'{1}\', \'{2}\', \'{3}\', \'{4}\', \'{5}\', {6})'.format(date_of_flight, location_of_flight, season, image_name, region_name, label, probability)
    logging.info(statement)
    execute(statement)

def insert_paragrass_result():
    pass
    
def execute(statement):
    cnxn = pyodbc.connect('DRIVER='+driver+';SERVER='+server+';PORT=1433;DATABASE='+database+';UID='+username+';PWD='+ password)
    cursor = cnxn.cursor()
    cursor.execute(statement) 
    cnxn.commit()