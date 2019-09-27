package azurerm

import (
	"fmt"

	"github.com/Azure/azure-sdk-for-go/services/machinelearningservices/mgmt/2019-06-01/machinelearningservices"
	"github.com/hashicorp/terraform/helper/validation"
	"github.com/terraform-providers/terraform-provider-azurerm/azurerm/helpers/suppress"
	"github.com/hashicorp/terraform/helper/schema"
	"github.com/terraform-providers/terraform-provider-azurerm/azurerm/helpers/azure"
	"github.com/terraform-providers/terraform-provider-azurerm/azurerm/helpers/tf"
	"github.com/terraform-providers/terraform-provider-azurerm/azurerm/internal/tags"
	"github.com/terraform-providers/terraform-provider-azurerm/azurerm/utils"
)

func resourceArmAmlWorkspace() *schema.Resource {
	return &schema.Resource{
		Create: resourceArmAmlWorkspaceCreateUpdate,
		Read:   resourceArmAmlWorkspaceRead,
		Update: resourceArmAmlWorkspaceCreateUpdate,
		Delete: resourceArmAmlWorkspaceDelete,

		Schema: map[string]*schema.Schema{
			"name": {
				Type:     schema.TypeString,
				Required: true,
			},

			"location": azure.SchemaLocation(),

			"resource_group_name": azure.SchemaResourceGroupName(),

			"description": {
				Type:     schema.TypeString,
				Required: true,
			},

			"friendly_name": {
				Type:     schema.TypeString,
				Required: true,
			},

			"key_vault": {
				Type:     schema.TypeString,
				Required: true,
			},

			"application_insights": {
				Type:     schema.TypeString,
				Required: true,
			},

			"container_registry": {
				Type:     schema.TypeString,
				Required: true,
			},

			"storage_account": {
				Type:     schema.TypeString,
				Required: true,
			},

			"discovery_url": {
				Type:     schema.TypeString,
				Required: true,
			},

			"tags": tags.Schema(),

			"identity": {
				Type:     schema.TypeList,
				Optional: true,
				Computed: true,
				MaxItems: 1,
				Elem: &schema.Resource{
					Schema: map[string]*schema.Schema{
						"type": {
							Type:             schema.TypeString,
							Required:         true,
							DiffSuppressFunc: suppress.CaseDifference,
							ValidateFunc: validation.StringInSlice([]string{
								string(machinelearningservices.SystemAssigned),
							}, false),
						},
						"principal_id": {
							Type:     schema.TypeString,
							Computed: true,
						},
						"identity_ids": {
							Type:     schema.TypeList,
							Optional: true,
							MinItems: 1,
							Elem: &schema.Schema{
								Type:         schema.TypeString,
								ValidateFunc: validation.NoZeroValues,
							},
						},
					},
				},
			},
		},
	}
}

func resourceArmAmlWorkspaceCreateUpdate(d *schema.ResourceData, meta interface{}) error {
	client := meta.(*ArmClient).machineLearning.WorkspacesClient
	ctx := meta.(*ArmClient).StopContext

	name := d.Get("name").(string)
	resGroup := d.Get("resource_group_name").(string)
	location := azure.NormalizeLocation(d.Get("location").(string))
	description := d.Get("description").(string)
	friendlyName := d.Get("friendly_name").(string)
	storageAccount := d.Get("storage_account").(string)
	keyVault := d.Get("key_vault").(string)
	containerRegistry := d.Get("container_registry").(string)
	applicationInsights := d.Get("application_insights").(string)
	discoveryUrl := d.Get("discovery_url").(string)
	t := d.Get("tags").(map[string]interface{})

	existing, err := client.Get(ctx, resGroup, name)
	if err != nil {
		if !utils.ResponseWasNotFound(existing.Response) {
			return fmt.Errorf("Error checking for existing AML Workspace %q (Resource Group %q): %s", name, resGroup, err)
		}

		if existing.ID != nil && *existing.ID != "" {
			return tf.ImportAsExistsError("azurerm_machine_learning_workspace", *existing.ID)
		}
	}

	workspace := machinelearningservices.Workspace{
		Name:     &name,
		Location: &location,
		Tags:     tags.Expand(t),
		WorkspaceProperties: &machinelearningservices.WorkspaceProperties{
			Description:         &description,
			FriendlyName:        &friendlyName,
			StorageAccount:      &storageAccount,
			DiscoveryURL:        &discoveryUrl,
			ContainerRegistry:   &containerRegistry,
			ApplicationInsights: &applicationInsights,
			KeyVault:            &keyVault,
		},
	}

	if _, ok := d.GetOk("identity"); ok {
		amlIdentity := expandAmlIdentity(d)
		workspace.Identity = amlIdentity
	}

	result, err := client.CreateOrUpdate(ctx, resGroup, name, workspace)
	if err != nil {
		return fmt.Errorf("Error during workspace creation %q in resource group (%q): %+v", name, resGroup, err)
	}

	fmt.Printf("created AML Workspace %q", result.Name)

	resp, err := client.Get(ctx, resGroup, name)
	if err != nil {
		return err
	}

	if resp.ID == nil {
		return fmt.Errorf("Cannot read workspace %q (resource group %q) ID", name, resGroup)
	}

	d.SetId(*resp.ID)

	return resourceArmAmlWorkspaceRead(d, meta)
}

func resourceArmAmlWorkspaceRead(d *schema.ResourceData, meta interface{}) error {
	client := meta.(*ArmClient).machineLearning.WorkspacesClient
	ctx := meta.(*ArmClient).StopContext

	id, err := azure.ParseAzureResourceID(d.Id())
	if err != nil {
		return err
	}

	resGroup := id.ResourceGroup
	name := id.Path["machineLearningServices"]

	resp, err := client.Get(ctx, resGroup, name)
	if err != nil {
		if utils.ResponseWasNotFound(resp.Response) {
			d.SetId("")
			return nil
		}
		return fmt.Errorf("Error making Read request on Workspace %q (Resource Group %q): %+v", name, resGroup, err)
	}

	d.Set("name", resp.Name)
	d.Set("resource_group_name", resGroup)
	if location := resp.Location; location != nil {
		d.Set("location", azure.NormalizeLocation(*location))
	}

	if props := resp.WorkspaceProperties; props != nil {
		d.Set("description", props.Description)
		d.Set("friendly_name", props.FriendlyName)
		d.Set("storage_account", props.StorageAccount)
		d.Set("discovery_url", props.DiscoveryURL)
		d.Set("container_registry", props.ContainerRegistry)
		d.Set("application_insights", props.ApplicationInsights)
		d.Set("key_vault", props.KeyVault)
	}

	return tags.FlattenAndSet(d, resp.Tags)
}

func resourceArmAmlWorkspaceDelete(d *schema.ResourceData, meta interface{}) error {
	client := meta.(*ArmClient).machineLearning.WorkspacesClient
	ctx := meta.(*ArmClient).StopContext

	id, err := azure.ParseAzureResourceID(d.Id())
	if err != nil {
		return err
	}

	resGroup := id.ResourceGroup
	name := id.Path["machineLearningServices"]

	_, err = client.Delete(ctx, resGroup, name)
	if err != nil {
		return fmt.Errorf("Error deleting workspace %q (Resource Group %q): %+v", name, resGroup, err)
	}

	return nil
}

func expandAmlIdentity(d *schema.ResourceData) *machinelearningservices.Identity {
	v := d.Get("identity")
	identities := v.([]interface{})
	identity := identities[0].(map[string]interface{})
	identityType := machinelearningservices.ResourceIdentityType(identity["type"].(string))

	amlIdentity := machinelearningservices.Identity{
		Type: identityType,
	}

	return &amlIdentity
}
