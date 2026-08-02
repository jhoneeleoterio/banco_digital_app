variable "project_name" {
  description = "Prefixo usado no nome dos recursos."
  type        = string
  default     = "bancodigital"
}

variable "location" {
  description = "Região do Azure onde os recursos serão criados."
  type        = string
  default     = "brazilsouth"
}

variable "resource_group_name" {
  description = "Nome do resource group."
  type        = string
  default     = "rg-bancodigital"
}

variable "postgres_admin_username" {
  description = "Usuário administrador do Postgres Flexible Server."
  type        = string
  default     = "bancodigital_admin"
}

variable "postgres_database_name" {
  description = "Nome do banco de dados dentro do servidor Postgres."
  type        = string
  default     = "banco_digital"
}

variable "postgres_sku_name" {
  description = "SKU do Postgres Flexible Server (B_Standard_B1ms é elegível à cota grátis de 12 meses)."
  type        = string
  default     = "B_Standard_B1ms"
}

variable "postgres_storage_mb" {
  description = "Armazenamento do Postgres em MB (32768 = 32GiB, dentro da cota grátis)."
  type        = number
  default     = 32768
}

variable "postgres_version" {
  description = "Versão major do PostgreSQL."
  type        = string
  default     = "16"
}

variable "container_image_tag" {
  description = "Tag da imagem já publicada no Container Registry (ex: latest, ou o SHA do commit)."
  type        = string
  default     = "latest"
}

variable "container_cpu" {
  description = "vCPUs alocadas ao container da API."
  type        = number
  default     = 0.25
}

variable "container_memory" {
  description = "Memória alocada ao container da API."
  type        = string
  default     = "0.5Gi"
}
