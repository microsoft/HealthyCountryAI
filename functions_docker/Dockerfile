# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/python:2.0-python3.7-appservice
FROM mcr.microsoft.com/azure-functions/python:2.0-python3.7

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

RUN sudo add-apt-repository ppa:ubuntugis/ppa && sudo apt-get update
RUN sudo apt-get update
RUN sudo apt-get install gdal-bin
RUN sudo apt-get install libgdal-dev
RUN export CPLUS_INCLUDE_PATH=/usr/include/gdal
RUN export C_INCLUDE_PATH=/usr/include/gdal
RUN pip install GDAL

COPY requirements.txt /
RUN pip install -r /requirements.txt

COPY . /home/site/wwwroot