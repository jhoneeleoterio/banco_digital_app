resource "azurerm_log_analytics_workspace" "this" {
  name                = "law-${var.project_name}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  sku               = "PerGB2018"
  retention_in_days = 30
}

resource "azurerm_container_app_environment" "this" {
  name                       = "${var.project_name}-env"
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id
}

resource "azurerm_container_app" "this" {
  name                         = "${var.project_name}-api"
  container_app_environment_id = azurerm_container_app_environment.this.id
  resource_group_name          = azurerm_resource_group.this.name
  revision_mode                = "Single"

  identity {
    type = "SystemAssigned"
  }

  registry {
    server   = azurerm_container_registry.this.login_server
    identity = "System"
  }

  secret {
    name                = "connectionstring-default"
    key_vault_secret_id = azurerm_key_vault_secret.connection_string.versionless_id
    identity            = "System"
  }

  template {
    min_replicas = 0
    max_replicas = 1

    container {
      name   = "${var.project_name}-api"
      image  = "${azurerm_container_registry.this.login_server}/${var.project_name}-api:${var.container_image_tag}"
      cpu    = var.container_cpu
      memory = var.container_memory

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name        = "ConnectionStrings__Default"
        secret_name = "connectionstring-default"
      }
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
}

# Precisa existir antes da 1ª revisão conseguir puxar a imagem do ACR.
resource "azurerm_role_assignment" "acr_pull" {
  scope                = azurerm_container_registry.this.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_container_app.this.identity[0].principal_id
}

# Precisa existir antes da 1ª revisão conseguir ler o secret do Key Vault.
resource "azurerm_role_assignment" "container_app_kv_secrets_user" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_container_app.this.identity[0].principal_id
}
