terraform {
  backend "azurerm" {
    //container_name       = "tstate"
    //storage_account_name = "tstate28432"
    //key                  = "terraform.tfstate"
  }
}

# configure the provider
provider "azurerm" {
  version = "~> 1.34"

  //use_msi = true
}

# create resource group
resource "azurerm_resource_group" "rg" {
  name     = "${var.prefix}-rg"
  location = var.location
  tags     = var.tags
}

# create KeyVault
resource "azurerm_key_vault" "kv" {
  name                        = "${var.prefix}-kv"
  location                    = "${azurerm_resource_group.rg.location}"
  resource_group_name         = "${azurerm_resource_group.rg.name}"
  enabled_for_disk_encryption = true
  tenant_id                   = var.tenantId

  sku_name = "standard"

  access_policy {
    tenant_id = var.tenantId
    object_id = var.keyVaultUserObjectId

    key_permissions = [
      "get",
    ]

    secret_permissions = [
      "get",
    ]

    storage_permissions = [
      "get",
    ]
  }

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
  }

  tags = var.tags
}

# create Storage Account for data
resource "azurerm_storage_account" "sa" {
  name                     = "${var.prefix}sa"
  resource_group_name      = "${azurerm_resource_group.rg.name}"
  location                 = "${azurerm_resource_group.rg.location}"
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags                     = var.tags
}

# create Storage Account for functions
resource "azurerm_storage_account" "fnsa" {
  name                     = "${var.prefix}fnsa"
  resource_group_name      = "${azurerm_resource_group.rg.name}"
  location                 = "${azurerm_resource_group.rg.location}"
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags                     = var.tags
}

# create app service plan for functions
resource "azurerm_app_service_plan" "fn" {
  name                = "${var.prefix}fn"
  resource_group_name = "${azurerm_resource_group.rg.name}"
  location            = "${azurerm_resource_group.rg.location}"
  kind                = "Linux" // I think this needs to be functionapp,linux but Terraform doesn't like that
  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

# create app service for functions
resource "azurerm_function_app" "fn" {
  name                      = "${var.prefix}fn"
  resource_group_name       = "${azurerm_resource_group.rg.name}"
  location                  = "${azurerm_resource_group.rg.location}"
  app_service_plan_id       = "${azurerm_app_service_plan.fn.id}"
  storage_connection_string = "${azurerm_storage_account.fnsa.primary_connection_string}"

  app_settings {
    APPINSIGHTS_INSTRUMENTATIONKEY = "${azurerm_application_insights.ai.instrumentation_key}"
    FUNCTIONS_EXTENSION_VERSION = "~2"
    FUNCTIONS_WORKER_RUNTIME = "python"
  }
}

# create Application Insights
resource "azurerm_application_insights" "ai" {
  name                = "${var.prefix}-ai"
  location            = var.location
  resource_group_name = "${azurerm_resource_group.rg.name}"
  application_type    = "web"
  tags                = var.tags
}

# create Container Registry
resource "azurerm_container_registry" "acr" {
  name                = "${var.prefix}acr"
  resource_group_name = "${azurerm_resource_group.rg.name}"
  location            = "${azurerm_resource_group.rg.location}"
  sku                 = "Standard"
  admin_enabled       = true
  tags                = var.tags
}

# create a machine learning workspace
resource "azurerm_template_deployment" "aml" {
  name                = "${var.prefix}-aml"
  resource_group_name = "${azurerm_resource_group.rg.name}"
  deployment_mode     = "Incremental"
  template_body       = "${file("../arm/azuredeploy_aml.json")}"
  parameters = {
    workspaceName         = "${var.prefix}-aml"
    location              = var.location
    storageAccountId      = "${azurerm_storage_account.sa.id}"
    keyVaultId            = "${azurerm_key_vault.kv.id}"
    applicationInsightsId = "${azurerm_application_insights.ai.id}"
    containerRegistryId   = "${azurerm_container_registry.acr.id}"
  }
}

# create cognitive services computer vision project for magpie geese
resource "azurerm_template_deployment" "vis-geese" {
  name                = "${var.prefix}-vis-geese"
  resource_group_name = "${azurerm_resource_group.rg.name}"
  deployment_mode     = "Incremental"
  template_body       = "${file("../arm/azuredeploy_vis.json")}"
  parameters = {
    accountName = "${var.prefix}-vis-geese"
    location    = var.location
    sku         = "S1"
  }
}

# create cognitive services computer vision project for parra grass
resource "azurerm_template_deployment" "vis-grass" {
  name                = "${var.prefix}-vis-grass"
  resource_group_name = "${azurerm_resource_group.rg.name}"
  deployment_mode     = "Incremental"
  template_body       = "${file("../arm/azuredeploy_vis.json")}"
  parameters = {
    accountName = "${var.prefix}-vis-grass"
    location    = var.location
    sku         = "S1"
  }
}

# outputs
output "app_insights_key" {
  value = "${azurerm_application_insights.ai.instrumentation_key}"
}

output "app_id" {
  value = "${azurerm_application_insights.ai.app_id}"
}

output "workspace_id" {
  value = "${azurerm_template_deployment.aml.outputs["workspaceId"]}"
}

/* output "cognitive_vision_geese_key" {
  depends_on = [azurerm_template_deployment.vis-geese, ]
  value      = "${lookup(azurerm_template_deployment.vis-geese.outputs, "cognitiveServicesKey")}"
}

output "cognitive_vision_geese_endpoint" {
  depends_on = [azurerm_template_deployment.vis-geese, ]
  value      = "${lookup(azurerm_template_deployment.vis-geese.outputs, "cognitiveServicesEndpoint")}"
}

output "cognitive_vision_grass_key" {
  depends_on = [azurerm_template_deployment.vis-grass, ]
  value      = "${lookup(azurerm_template_deployment.vis-grass.outputs, "cognitiveServicesKey")}"
}

output "cognitive_vision_grass_endpoint" {
  depends_on = [azurerm_template_deployment.vis-grass, ]
  value      = "${lookup(azurerm_template_deployment.vis-grass.outputs, "cognitiveServicesKey")}"
} */

/* resource "azurerm_machine_learning_workspace" "aml" {
  name                 = "${var.prefix}-aml"
  location             = var.location
  resource_group_name  = "${azurerm_resource_group.rg.name}"
  description          = "test aml workspace"
  friendly_name        = "test aml workspace"
  key_vault            = "${azurerm_key_vault.kv.id}"
  storage_account      = "${azurerm_storage_account.sa.id}"
  application_insights = "${azurerm_application_insights.ai.id}"
  container_registry   = "${azurerm_container_registry.acr.id}"
  discovery_url        = "http://test.com"
  tags = {
    environment = var.environment
  }
} */
