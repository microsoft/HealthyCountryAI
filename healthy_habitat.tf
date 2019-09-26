terraform {
  backend "azurerm" {
    resource_group_name  = "Deployment"
    storage_account_name = "jodownscsirodeploy"
    container_name       = "terraform"
    key                  = "prod.terraform.tfstate"
  }
}

# Configure the provider
provider "azurerm" {
    version = "=1.27.0"
}

# Create a new resource group
resource "azurerm_resource_group" "rg" {
    name     = "HealthyHabitat"
    location = "australiaeast"
}