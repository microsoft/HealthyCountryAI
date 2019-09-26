variable "subscriptionId" {
  default = "b2375b5f-8dab-4436-b87c-32bc7fdce5d0"
}

variable "tenantId" {
  default = "3d49be6f-6e38-404b-bbd4-f61c1a2d25bf"
}

variable "location" {
  default = "australiaeast"
}

variable "keyVaultUserObjectId" {
  default = "57963f10-818b-406d-a2f6-6e758d86e259"
}

variable "prefix" {
  default = "healthyhabitat"
}

variable "tags" {
  type = "map"
    default = {
        "environment"  = "dev"
        "project" = "HealthyHabitat"
        "costcentre" = "298742394"
    }
}
