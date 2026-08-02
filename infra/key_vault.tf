resource "azurerm_key_vault" "this" {
  name                = "kv-${var.project_name}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  tenant_id = data.azurerm_client_config.current.tenant_id
  sku_name  = "standard"

  rbac_authorization_enabled = true
}

# Permite que quem está rodando o `terraform apply` (você, via az login) grave secrets.
resource "azurerm_role_assignment" "deployer_secrets_officer" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azurerm_client_config.current.object_id
}

locals {
  postgres_connection_string = join(";", [
    "Host=${azurerm_postgresql_flexible_server.this.fqdn}",
    "Port=5432",
    "Database=${azurerm_postgresql_flexible_server_database.this.name}",
    "Username=${var.postgres_admin_username}",
    "Password=${random_password.postgres_admin.result}",
    "Ssl Mode=Require",
  ])
}

resource "azurerm_key_vault_secret" "connection_string" {
  name         = "connectionstring-default"
  value        = local.postgres_connection_string
  key_vault_id = azurerm_key_vault.this.id

  depends_on = [azurerm_role_assignment.deployer_secrets_officer]
}
