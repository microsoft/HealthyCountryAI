variable "tenantId" {
  default = "de85fb3a-913e-44fd-9b1e-982d9076b94a"
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
