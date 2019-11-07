variable "tenantId" {
  default = ""
}

variable "location" {
  default = "australiaeast"
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
