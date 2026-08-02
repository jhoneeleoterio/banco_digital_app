resource "random_password" "postgres_admin" {
  length      = 24
  special     = false # Npgsql connection string não escapa caracteres especiais na senha
  min_upper   = 1
  min_lower   = 1
  min_numeric = 1
}

resource "azurerm_postgresql_flexible_server" "this" {
  name                = "${var.project_name}-pg"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  version  = var.postgres_version
  sku_name = var.postgres_sku_name
  zone     = "1"

  storage_mb = var.postgres_storage_mb

  administrator_login    = var.postgres_admin_username
  administrator_password = random_password.postgres_admin.result

  public_network_access_enabled = true

  # SKU Burstable não suporta alta disponibilidade — mantém o custo dentro da cota grátis.
  lifecycle {
    ignore_changes = [zone]
  }
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure_services" {
  name             = "AllowAllAzureServicesAndResourcesWithinAzureIps"
  server_id        = azurerm_postgresql_flexible_server.this.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_postgresql_flexible_server_database" "this" {
  name      = var.postgres_database_name
  server_id = azurerm_postgresql_flexible_server.this.id
  collation = "en_US.utf8"
  charset   = "UTF8"
}
