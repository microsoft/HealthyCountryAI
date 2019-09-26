terraform {
  backend "azurerm" {
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