Param(
    $Location = 'australiaeast',
    $ProjectName = 'hcai',
    $ResourceGroupName = $('{0}-{1}' -f $ProjectName, 'rg'),
    $DeploymentName = $('{0}-{1}' -f $ProjectName, $(Get-Date -Format yyyyMMddhhmmss)),
    $ContainerNames = @('cannonhill-bangkerreng', 'cannonhill-kunumeleng', 'cannonhil-wurrkeng', 'jabirudreaming-bangkerreng', 'jabirudreaming-kunumeleng', 'jabirudreaming-wurrkeng', 'ubir-bangkerreng', 'ubir-kunumeleng', 'ubir-wurrkeng')
)

function Invoke-SqlScript {
    Param(
        $ScriptPath,
        $SqlServerName,
        $SqlDatabaseName,
        $SqlServerAdminLoginName,
        $SqlServerAdminPassword
    )

    $ErrorActionPreference = 'Stop'
    $sqlScript = Get-Content -Path $ScriptPath
    $connString = 'Server=tcp:{0}.database.windows.net;Database={3};User ID={1}@{0};Password={2};Trusted_Connection=False;Encrypt=True;' -f $SqlServerName, $SqlServerAdminLoginName, $SqlServerAdminPassword, $SqlDatabaseName
    $sqlConn = [System.Data.SqlClient.SqlConnection]::new($connString)

    try {
        $sqlConn.Open()
        $command = [System.Data.SqlClient.SqlCommand]::new()
        $command.Connection = $sqlConn
        $command.CommandText = $sqlScript
        $command.ExecuteNonQuery()
    }
    catch {
        Write-Host "Error executing SQL statement: '$($_.Exception)'"
    }
    finally {
        $sqlConn.Close()
    }
}

# create resource group
New-AzDeployment `
    -Name $deploymentName `
    -Location $location `
    -TemplateFile $PSScriptRoot/../arm/resourcegroup.json `
    -ResourceGroupName $ResourceGroupName `
    -ResourceGroupLocation $location

# get client IP to set Azure SQL DB firewall access rulle
$clientIP = (Invoke-WebRequest -Uri 'http://whatismyip.akamai.com' -ErrorAction Stop).Content

# create resources
$resourceDeploymentResult = New-AzResourceGroupDeployment `
    -Name $deploymentName `
    -ResourceGroupName $ResourceGroupName `
    -Mode Incremental `
    -TemplateFile $PSScriptRoot/../arm/azuredeploy.json `
    -containerNames $containerNames `
    -sqlServerFWClientIpStart $clientIP `
    -sqlServerFWClientIpEnd $clientIP `
    -DeploymentDebugLogLevel All

if ($null -ne $resourceDeploymentResult) {
    $sqlServerName = $resourceDeploymentResult.Outputs.sqlServerName.Value
    $sqlDatabaseName = $resourceDeploymentResult.Outputs.sqlDatabaseName.Value
    $sqlServerAdminLoginName = $resourceDeploymentResult.Outputs.sqlServerAdminLoginName.Value
    $sqlServerAdminPassword = $resourceDeploymentResult.Outputs.sqlServerAdminLoginPassword.Value
    $functionAppName = $resourceDeploymentResult.Outputs.functionAppName.Value
    $storageAccountName = $resourceDeploymentResult.Outputs.storageAccountName.Value
} else {
    throw "`$resourceDeploymentResult is null"
}

# create databse tables
Invoke-SqlScript `
    -ScriptPath "$PSScriptRoot\tables.sql" `
    -SqlServerName $sqlServerName `
    -SqlDatabaseName $sqlDatabaseName `
    -SqlServerAdminLoginName $sqlServerAdminLoginName `
    -SqlServerAdminPassword $sqlServerAdminPassword

# push python code to Azure function
Set-Location $PSScriptRoot/../functions
func azure functionapp publish $functionAppName --python --force

# create eventGrid subscriptions
New-AzResourceGroupDeployment `
    -Name $deploymentName `
    -ResourceGroupName $ResourceGroupName `
    -Mode Incremental `
    -TemplateFile $PSScriptRoot/../arm/eventgrid.json `
    -ContainerNames $containerNames `
    -FunctionAppName $functionAppName `
    -StorageAccountName $storageAccountName `
    -DeploymentDebugLogLevel All