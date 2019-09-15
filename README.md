# Healthy Habitat AI

### Overview
The Healthy Habitat AI project consists of four models developed using CustomVision.ai and Azure Machine Learning Service using images and muti-spectral data, collected by rangers using DJI Mavic 2s from sites in Kakadu National Park. The models increase the area a ranger is able to assess, by converting large volumes of data into metrics significant to the health of the land.

### Data Preparation
The project consists of two Azure Storage accounts for data collection -
* Healthy Habitat Animals (healthyhabitatanimals), and
* Healthy Habitat Para grass (healthyhabitatparragrass)

Each account uses Blobs only.

Data is divided into folders named using the *site* the data was collected from and the *season* the data was collected during, for example -

* cannon-hill-kunumeleng
* cannon-hill-wurrkeng
* jabiru-dreaming-kunumeleng
* jabiru-dreaming-wurrkeng
* ubir-kunumeleng
* ubir-wurrkeng

Data preparation follows the process -

* Copy images from a DJI Mavic 2 to the local workstation
* 


### Projects

CustomVision.ai -
* Para grass - Classification
* Magpie Geese - Object Detection

Azure Machine Learning Service -
* Para grass - Semantic Segmentation (U-Net)
* Para grass - Multi-spectral (5-bands)

### Machine Learning Workstation
* Install [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/)
* Install [Power BI Desktop](https://powerbi.microsoft.com/en-us/desktop/)
* Install [Anaconda](https://docs.anaconda.com/anaconda/install/)
* Clone `https://github.com/svanbodegraven/HealthyHabitatAI.git`
* `cd HealthyHabitatAI`
* Configure a [local development environment](https://docs.microsoft.com/en-us/azure/machine-learning/service/how-to-configure-environment#local)

```
conda env create -f environment.yml
conda activate HealthyHabitatAI
conda install notebook ipykernel
ipython kernel install --user
```

### Field  Local Workstation
* Install [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/)
* Install [Power BI Desktop](https://powerbi.microsoft.com/en-us/desktop/)