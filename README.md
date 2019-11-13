# Healthy Country AI

### Overview
The Healthy Country project in Kakadu is a collaboration between Bininj co-researchers and Indigenous rangers, Kakadu Board of Management, [CSIRO](https://www.csiro.au/), [Parks Australia](https://parksaustralia.gov.au/), [Northern Australia National Environment Science Program (NESP)](https://www.nespnorthern.edu.au/), [University of Western Australia (UWA)](https://www.uwa.edu.au/), [Charles Darwin University (CDU)](https://www.cdu.edu.au/) and [Microsoft](https://www.microsoft.com/en-us/ai/ai-for-earth) to support better decision-making to care for significant species and habitats on Indigenous lands.

The Healthy Habitat AI project consists of three models developed using CustomVision.ai and Azure Machine Learning Service, using RGB images, collected by rangers using an off the shelf affordable drone, DJI Mavic PRO 2, from sites in Kakadu National Park, Australia. The models allow rangers to regularly survey large areas that are difficult to access by converting large volumes of data (1000s of high res photos) into metrics that demonstrate how the identified key values are changing following selected a management methods. The Healthy Habitat AI project represents an end to end solution to support adaptive management with clearly defined success metrics.

The models consist of -

CustomVision.ai:
* Para grass - Classification (304 x 228 px Tiles)
* Magpie Geese - Object Detection (304 x 228 px Tiles)

Azure Machine Learning Service:
* Para grass - Semantic Segmentation (U-Net)

### Data Preparation

![](app/HealthyHabitat/Images/Architecture.jpg)

The rangers and traditional owners, Bininj, selected several sites based on important environmental and cultural values. At each of these sites, a fixed set of transects are programmed into the drone to cover the same area of interest at the same height (60m above from point of take off) each time the area is flown. The drone flys at a set speed and sets capture rate for photos to get a 60% overlap in photos to allow photogramtic stitching. The three areas are flat flood plains so the height remains constant above the survey area. Once the transects are flown the rangers return to home base where they have internet and computers. The Micro-SD card is removed from the drone and inserted into the rangers pc.

Rangers seperate the photos into folders for each site and if multiple surveys have been flown at a site into site/date folders. An application is installed on the rangers pc or field laptop, opened through a short cut executable on the desktop. This application depicts the six seasons defined by environmental indicators that mark changes to the season. Rangers select the files from the site folders they created and drag the files into the appropriate season on the app. The app prompts the ranger to select a site from a list and then prompts to select the type of photographs, animal or habitat. Once the site and type are selected the app automatically synchronizes the data to Azure Storage and creates a standardised file structure for each site, site-season-type-datetime.

![](app/HealthyHabitat/Images/SeasonalWheel.png)

Storage consists of two accounts for data collection -
* Healthy Habitat Animals (healthyhabitatanimals), and
* Healthy Habitat Para grass (healthyhabitatparragrass)

Each account uses Blobs only.

Data is automatically divided into containers named using the combination of *site* and *season* matching the section of the seasonal wheel images were dragged onto, for example -

* cannon-hill-kunumeleng
* cannon-hill-wurrkeng
* jabiru-dreaming-kunumeleng
* jabiru-dreaming-wurrkeng
* ubir-kunumeleng
* ubir-wurrkeng

Then by the YYY-MM-DD-HHMM the collection occured, for example -
* 2019-04-03-1050

...

Classes -
* Para grass
* Burnt Para-grass
* Dense Para-grass
* Dead Para-grass
* Wet Para-grass
* Tree
* Water

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
