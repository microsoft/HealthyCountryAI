# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/python:2.0-python3.7-appservice
#!/usr/bin/env bash
FROM mcr.microsoft.com/azure-functions/python:2.0-python3.7-appservice

RUN apt-get update
RUN apt-get install -y python-numpy gdal-bin libgdal-dev g++

RUN echo $(gdal-config --version)

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY requirements.txt /
RUN pip install -r /requirements.txt

COPY . /home/site/wwwroot
