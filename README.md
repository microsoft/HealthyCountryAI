 ## Healthy Country AI

### Overview
The Healthy Country AI project in Kakadu National Park is a collaboration between Bininj co-researchers and Indigenous rangers, the [Kakadu Board of Management](https://www.directory.gov.au/portfolios/environment-and-energy/director-national-parks/kakadu-board-management), [CSIRO](https://www.csiro.au/), [Parks Australia](https://parksaustralia.gov.au/), [Northern Australia National Environment Science Program (NESP)](https://www.nespnorthern.edu.au/), the [University of Western Australia (UWA)](https://www.uwa.edu.au/), [Charles Darwin University (CDU)](https://www.cdu.edu.au/) and [Microsoft](https://www.microsoft.com/en-us/ai/ai-for-earth) to support better decision-making to care for significant species and habitats on Indigenous lands.

The Healthy Country AI project consists of three models developed using [Custom Vision](http://customvision.ai) and [Azure Machine Learning Service](https://azure.microsoft.com/en-us/services/machine-learning/).  The models operate on RGB images collected by rangers using an off-the-shelf affordable drone (DJI Mavic Pro 2) from sites in Kakadu National Park, Australia. The models allow rangers to regularly survey large areas that are difficult to access by converting large volumes of data (thousands of high-resolution photos) into metrics that demonstrate how the key metrics of ecosystem health are changing following management interventions. The Healthy Country AI project represents an end-to-end solution to support adaptive management with clearly defined success metrics.

The three models are:

* Para grass (classification, 304 &times; 228 tiles) (Custom Vision)
* Magpie Geese (object detection, 304 &times; 228 tiles) (Custom Vision)
* Para grass (semantic segmentation) (Azure Machine Learning Service)

![](Architecture.jpg)

## Responsible AI and ethical data governance

The AI which interprets the drone-collected data has been built with the advice and guidance of the Traditional Owners in regard to what is important to them from a land management perspective. 
That could be, for example, information about the presence or absence of animals, or information about the condition of different types of grasses and trees. 

The results and analysis are delivered to rangers via a Power BI dashboard that was designed in partnership with the Traditional Owners. Rangers can use the dashboard to support their decision making regardless of where they are based.

The solution has been constructed with several layers of privacy, as some of the sites where the drones collect data are sacred to the Traditional Owners, and as such, imagery and data from those sites needs to be properly protected.

The platform features three rings of data management and data governance:

* The innermost data ring is restricted to Traditional Owners, rangers, and Indigenous elders who identify which data can be made available to the second ring.
* The data in the second ring can be accessed by researchers and collaboration partners.
* The outermost ring is data that can be made available to the public.


# Data preparation

The rangers and Traditional Owners, Bininj, selected several sites based on important environmental and cultural values. At each of these sites, a fixed set of transects are programmed into the drone to cover the same area of interest at the same height (60m above the point of take-off) each time the area is flown. The drone flies at a set speed and sets its capture rate to get a 60% overlap between photos, to allow photogrammetric analysis. The three areas are flat flood plains, so the height remains constant above the survey area. Once the transects are flown, the rangers return to home base where they have Internet access. The Micro-SD card is removed from the drone and inserted into the ranger's PC.

Rangers separate the photos into folders for each site and each survey. An [https://github.com/microsoft/HealthyCountryAI/tree/master/app/Release](application) is installed on the ranger's PC; this application depicts the six seasons defined by environmental indicators. Rangers select the files from the site folders they created and drag the files into the season that Bininj Traditional Owners use to monitor and manage this area. The app prompts the ranger to select a site from a list and then prompts the user to select the type of photographs (animal or habitat). Once the site and type are selected, the app automatically synchronizes the data to Azure Storage and creates a standardised file structure for each site (site-season-type-datetime).

<img src="app/HealthyHabitat/Images/SeasonalWheel.png" width=500/><br/>

One storage account is used and subfolders are created to differentiate among sites, seasons, survey times, and model types. Files are stored as blobs. Data is automatically divided into containers named using the combination of *site* and *season*, matching the section of the seasonal wheel images were dragged onto, for example:

* cannon-hill-kunumeleng
* cannon-hill-wurrkeng
* jabiru-dreaming-kunumeleng
* jabiru-dreaming-wurrkeng
* ubir-kunumeleng
* ubir-wurrkeng

...then by the YYY-MM-DD-HHMM the collection occurred, for example:

* 2019-04-03-1050

Two functions are triggered by the successful upload of each photo:

<i>Function 1: Split regions</i>

The first function splits each photograph into 120 tiles and uploads the tiled images to seasonal projects in Custom Vision for labeling. If a project doesn't exist, a new Custom Vision project is automatically created using the site-season-type combination described above. End users can then open Custom Vision and label animals (object detection) or habitat (classification) to train the model.  As more labels are accumulated for each model, users should train the model and publish the new results to improve accuracy.

<i>Function 2: Score regions</i>

This function first resizes the high-resolution images (5472 &times; 3648) and saves a smaller (1024 &times; 768) version of the photo with the same name to support quicker rendering on the Power BI dashboard.  The function then uses the available models in Custom Vision to score each tile for dominant habitat type (classification) and animal (object detection). The scores are written out to an SQL database with associated covariates that allow subsequent filtering and analytics in Power BI. In this system a link to a SAS URL for each image is written to the database to provide a direct link back to the photographs using survey date as the key.

# AI/ML models

Here we have implemented three models: classification, object detection (using Custom Vision) and semantic segmentation (using Azure Machine Learning Service).  This approach requires users to train models to identify objects in the photos by labelling features of interest. There is no fixed number of labels required to train the models, but we found that once 2000 images were labelled, the accuracy of the models greatly increased and the labelling task became faster and easier.  In Custom Vision, once you have completed the initial labelling task, the models are trained, allowing predictions to be displayed on the untagged images.  The user can then confirm or reject the predicted label and add any missed features.   

## Custom Vision models

### Habitat

For the habitat model, we scored the dominant habitat type for each tile by season and site. We greatly reduced the complexity of the labelling task by limiting the labels to broad habitat types, with more detail provided for our target species, para grass, including a "dead para grass" label which directly relates to the management goals of the rangers and Traditional Owners. We chose to label eight broad habitat categories;

* Para grass
* Water
* Dense para-grass
* Dead para-grass
* Bare ground
* Other grass
* Lily
* Tree

We use a single tag per image for our classification model. This required subject matter experts, in this case researchers who had a good knowledge of the visual characteristics of para grass compared with other native species from aerial photos. Using this method, it was necessary make decisions about which habitat type was dominant, reducing the complexity of the labelling task but also reducing the detail of the results and leading to difficult labelling decisions in tiles that had diverse habitat characteristics, e.g equal parts water, bare ground, and paragrass.

### Animal

For the animal model, subject matter experts (researchers with deep knowledge of vertebrate species in the region) labelled all vertebrate species that were easily identified in the tiles. The focus species &ndash; magpie geese &ndash; was easily separated from other species, so there was a very high confidence in these labels. Other species, such as egrets and spoon bills, were less distinct (from 60m) and were lumped into one category &ndash; egrets &ndash; which included all white birds. Several other species had very few individuals (<15 labels) and these were excluded. Labels were dominated by magpie geese and egrets; the remaining species were sparse. Species labels include:

* Goose
* Egret
* Crocodile
* Stork
* Darter (<15 labels)
* Kite (<15 labels)


# How to label data for segmentation

Semantic segmentation requires categories to be outlined using polygons for each category visible in a image.  For this project, we used [LabelBox](https://labelbox.com/) for creating polygon annotations. 

LabelBox is linked directly to the Azure Blob storage containers using a SAS token.  Polygons are created by either (1) holding down the left mouse button and tracing around the habitat or (2) clicking new vertices.  The availability of both tools provides an user experience that allows the user to choose coarse or fine methods depending on the feature that is being labelled.  

Despite the user-friendly labelling process, it is still difficult to categorise complex, continuous habitat features.  Some habitat types are heterogeneous,  so the user needs to make a choice as to what the dominant category is, understanding that sometimes there is not a clear category.  There are also many small habitat features, such as waterholes surround by other categories, that are time consuming to mask.  The labelling experience is easiest when there are distinct separated features such as open water, bare ground, or a dense monoculture of para grass.  When there are largely homogeneous habitat types, the quickest way to label the image is to label the least dominant categories using the polygon tool and then use the drawing tool to label the remaining parts of the image.  This ensures that edges between categories are completely labelled and discrete.  It is important to ensure that the barriers between habitat categories are closed before using the drawing tool, otherwise any connected space will be filled with the selected category.  

A useful feature in LabelBox is that the user can zoom in to an area and partially enclose a particular category, then add to the polygon by using overlapping polygons. This allows more detailed labelling to occur.   Using this method it is also possible to enclose the edges of a category, label other categories within the enclosed area, and then use the fill tool to easily label the dominant category.  

![](app/HealthyHabitat/Images/labelBox.PNG)

<i>Example of LabelBox with overlapping habitat types. The remaining category (water) can be easily labelled with the fill tool (paint drop icon).</i>

# Guidelines for selecting categories

The main challenge using this approach is to decide how many categories to label.  To develop an accurate model, each category requires many labels (thousands).  Therefore, if you try to be too specific with the categories (e.g 10 categories to describe the different growth states of para grass), you will need to multiply the number of labels required by the number of categories you created. For example, if you are aiming for a minimum of 2000 labels for each category, five categories will require 10000 labels.  Conversely, if you select more general and inclusive labels (e.g lump all types of grass into an "all grasses" label), you will require fewer labels, but your model will not be very specific.  

# Data visualisation and interaction

The scored data is stored in a SQL database which is linked to a Power BI report.  The SQL server contains links to the resized photograph and the scored results of the Custom Vision models (i.e., the percentage of each habitat type and count of each animal).

![](Dashboard_paragrass.PNG)

# Running the tools

## Configuring the ML workstation

* Install [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/)
* Install [Power BI Desktop](https://powerbi.microsoft.com/en-us/desktop/)
* Install [Anaconda](https://docs.anaconda.com/anaconda/install/)
* Clone [github.com/svanbodegraven/HealthyHabitatAI.git](https://github.com/svanbodegraven/HealthyHabitatAI.git)
* Configure a [local development environment](https://docs.microsoft.com/en-us/azure/machine-learning/service/how-to-configure-environment#local)

```
conda env create -f environment.yml
conda activate HealthyHabitatAI
conda install notebook ipykernel
ipython kernel install --user
```

## Accessing the ML models

Two options are provided:

* A [fast.ai Jupyter notebook](./notebooks/ubir-wurrkeng-fastai.ipynb), and
* An [ML Pipeline](./notebooks/qubvel-segmentation_models-U-Net-ML-Pipeline.ipynb) based on [github.com/qubvel/segmentation_models](https://github.com/qubvel/segmentation_models)

Both the fast.ai Jupyter notebook and the ML Pipeline use data prepared with the data preparation notebook and [Labelbox](https://labelbox.com).

### Configuring the field workstation

* Install [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/)
* Install [Power BI Desktop](https://powerbi.microsoft.com/en-us/desktop/)

# Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [cla.microsoft.com](https://cla.opensource.microsoft.com/).

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Contact

For more details or help deploying, contact:
* [Tianji Dickens](Tianji.Dickens@microsoft.com), Microsoft
* [Dr Justin Perry]( Justin.perry@csiro.au), CSIRO
* [Steve van Bodegraven](Steve.VanBodegraven@microsoft.com), Microsoft

# License

This repository is licensed with the [MIT license](https://github.com/Microsoft/dotnet/blob/master/LICENSE).
