# AgroSolutions - API de Cadastro de Talhões

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📋 Sobre o Projeto

A **AgroSolutions** é uma API REST desenvolvida para gerenciamento de propriedades agrícolas e seus respectivos talhões. O sistema permite que produtores rurais cadastrem, consultem, atualizem e gerenciem suas propriedades e subdivisões de terra (talhões) de forma eficiente e segura.

### Funcionalidades Principais

- ✅ Gerenciamento completo de propriedades agrícolas
- ✅ Cadastro e controle de talhões por propriedade
- ✅ Autenticação e autorização via JWT
- ✅ Soft delete (exclusão lógica)
- ✅ Versionamento de API
- ✅ Documentação automática com Swagger
- ✅ Logs estruturados com Serilog
- ✅ Integração com Azure Key Vault
- ✅ Monitoramento com Application Insights
- ✅ Cache em memória para otimização de performance

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture** e está organizado em camadas:

```
AgroSolutions/
├── 📁 src/
│   ├── 📁 AgroSolutions.Api/           # 🎯 Camada de Apresentação
│   │   ├── 📁 Configs/                 # Configurações (JWT, Versioning, etc)
│   │   ├── 📁 Controllers/             # Controllers organizados por versão
│   │   │   └── 📁 v1/                 # 📌 Endpoints versão 1
│   │   ├── 📁 Dtos/                   # Data Transfer Objects
│   │   ├── 📁 Extensions/             # 🔧 Extensões de configuração
│   │   ├── 📁 MappingDtos/            # 🔄 Mapeamento Entidade ↔ DTO
│   │   └── 📁 Middleware/             # 🛡️ Middlewares personalizados
│   │
│   ├── 📁 AgroSolutions.Business/     # 💼 Camada de Negócio
│   │   ├── 📁 Aplicacao/              # 🧠 Services e Interfaces
│   │   └── 📁 Dominio/                # 🎯 Entidades de domínio
│   │
│   ├── 📁 AgroSolutions.Infra/        # 🔧 Camada de Infraestrutura
│   │   ├── 📁 Cache/                  # ⚡ Implementações de cache
│   │   ├── 📁 Data/                   # 🗄️ Entity Framework & Migrations
│   │   ├── 📁 IoC/                    # 📦 Dependency Injection
│   │   └── 📁 Token/                  # 🔐 JWT Token Service
│   │
│   └── 📁 AgroSolutions.Core/         # 🌐 Componentes Compartilhados
│
└── 📁 Tests/
    └── 📁 AgroSolutions.Tests/        # 🧪 Testes Automatizados
```

## 🚀 Tecnologias Utilizadas

- **Framework**: .NET 8.0
- **ORM**: Entity Framework Core 8.0.16
- **Autenticação**: JWT Bearer Authentication
- **Logging**: Serilog com enrichers de Correlation ID
- **Documentação**: Swagger/OpenAPI
- **Versionamento**: Asp.Versioning.Mvc 8.1.0
- **Cache**: Microsoft.Extensions.Caching.Memory
- **Cloud**: Azure Key Vault, Application Insights
- **Container**: Docker

## 📦 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (opcional, para execução em container)
- [SQL Server](https://www.microsoft.com/sql-server) ou outro banco compatível com EF Core
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/) (recomendado)

## ⚙️ Configuração e Execução

### **1. Clonar o Repositório**

```bash
git clone https://github.com/Grupo-Postech-Fiap05/api-cadastro-talhao.git
cd api-cadastro-talhao
```

### **2. Configurar o appsettings.json**

Edite o arquivo `src/AgroSolutions.Api/appsettings.json` com suas configurações:

```json
{
  "KeyVault": {
    "Url": "https://seu-keyvault.vault.azure.net/"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-jwt",
    "Issuer": "FIAP",
    "Audience": "FIAP-API-FASE-05",
    "DurationInMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AgroSolutions;User Id=sa;Password=SuaSenha;"
  },
  "AdminUser": {
    "Email": "admin@fiap.com.br",
    "Password": "Pa$$w0rd",
    "Nome": "Administrador do Sistema"
  }
}
```

### **3. Executar Migrations**

```bash
cd src/AgroSolutions.Api
dotnet ef database update
```

### **4. Restaurar Dependências & Executar**

```bash
# Restaurar pacotes
dotnet restore

# Executar aplicação
dotnet run --project src/AgroSolutions.Api/AgroSolutions.Api.csproj
```

### **5. Acessar a Aplicação**
- 🌐 **API Base**: `https://localhost:7000`
- 📖 **Swagger Documentation**: `https://localhost:7000/swagger`

## 🐳 Executando com Docker

### Build da imagem

```bash
docker build -t agrosolutions-api .
```

### Executar o container

```bash
docker run -d -p 8080:8080 --name agrosolutions agrosolutions-api
```

## 📚 Documentação da API

Após iniciar a aplicação, acesse a documentação interativa do Swagger:

```
https://localhost:7000/swagger
```

### Principais Endpoints

#### 🏡 Propriedades

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/api/cadastro-agricola/propriedades` | Lista todas as propriedades do usuário | ✅ |
| GET | `/api/cadastro-agricola/propriedades/{id}` | Obtém uma propriedade específica | ✅ |
| POST | `/api/cadastro-agricola/propriedades` | Cria uma nova propriedade | ✅ |
| PUT | `/api/cadastro-agricola/propriedades/{id}` | Atualiza uma propriedade | ✅ |
| DELETE | `/api/cadastro-agricola/propriedades/{id}` | Remove uma propriedade (soft delete) | ✅ |

#### 🌾 Registro Sensor

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| POST | `/api/sensores/talhao/ObterStatus` | Lista todos os talhões de uma propriedade | ✅ |
| POST | `/api/sensores/talhao/ObterDados` | Lista os talhões por filtros de data, propriedade e talhão | ✅


#### 🌾 Talhões

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/api/cadastro-agricola/propriedades/{propriedadeId}/talhoes` | Lista todos os talhões de uma propriedade | ✅ |
| GET | `/api/cadastro-agricola/propriedades/{propriedadeId}/talhoes/{id}` | Obtém um talhão específico | ✅ |
| POST | `/api/cadastro-agricola/propriedades/{propriedadeId}/talhoes` | Cria um novo talhão | ✅ |
| PUT | `/api/cadastro-agricola/propriedades/{propriedadeId}/talhoes/{id}` | Atualiza um talhão | ✅ |
| DELETE | `/api/cadastro-agricola/propriedades/{propriedadeId}/talhoes/{id}` | Remove um talhão (soft delete) | ✅ |

### Exemplo de Requisições

#### Criar uma Propriedade

```bash
curl -X POST https://localhost:7000/api/cadastro-agricola/propriedades \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer seu-token-jwt" \
  -d '{
    "nome": "Fazenda Santa Maria",
    "localizacao": "São Paulo, SP",
    "areaTotal": 1500.5
  }'
```

#### Criar um Talhão

```bash
curl -X POST https://localhost:7000/api/cadastro-agricola/propriedades/{propriedadeId}/talhoes \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer seu-token-jwt" \
  -d '{
    "nome": "Talhão A",
    "area": 250.5,
    "cultura": "Soja"
  }'
```

## 🛠️ Recursos Técnicos Implementados

### ⚡ **Sistema de Cache**
- Cache em memória para otimização de performance
- Redução de consultas ao banco de dados

### 🔍 **Correlation ID**
- Rastreamento único para cada requisição
- Facilita debugging em ambiente de produção
- Header personalizado: `X-Correlation-ID`

### 📝 **Sistema de Logging**
- Serilog para logging estruturado
- Logs salvos em arquivo e console
- Integração com Application Insights (Azure)

### 🛡️ **Global Exception Handler**
- Tratamento centralizado de exceções
- Retorno padronizado de erros
- Logging automático de erros críticos

### 🔐 **Autenticação JWT**
- Tokens JWT para autenticação stateless
- Claims-based authorization
- Controle de acesso por usuário

### 📱 **Versionamento de API**
- Suporte a múltiplas versões da API
- Versionamento por URL: `/api/v1/`, `/api/v2/`
- Documentação separada por versão

## 🔐 Autenticação

A API utiliza **JWT (JSON Web Tokens)** para autenticação. Para acessar os endpoints protegidos:

1. Obtenha um token JWT através do endpoint de autenticação
2. Inclua o token no header de todas as requisições:

```
Authorization: Bearer seu-token-jwt-aqui
```

## 🧪 Testes

### Executar os testes

```bash
dotnet test
```

### Executar com cobertura

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 📊 Monitoramento e Logs

### Logs

A aplicação utiliza **Serilog** para logging estruturado. Os logs são salvos em:

- **Console**: Para desenvolvimento
- **Arquivo**: `logs/log-{Date}.txt`
- **Application Insights**: Para produção (se configurado)

### Correlation ID

Cada requisição recebe um `X-Correlation-ID` único para rastreamento distribuído e facilitação de debugging.

## 🌐 Variáveis de Ambiente

Para produção, considere usar variáveis de ambiente ao invés de appsettings.json:

```bash
export Jwt__Key="sua-chave-jwt"
export ConnectionStrings__DefaultConnection="sua-connection-string"
export KeyVault__Url="https://seu-keyvault.vault.azure.net/"
```

## 👥 Equipe - Grupo Postech FIAP - Fase 05

- 👨‍💻 Clovis Alceu Cassaro (cloves_93258)
- 👨‍💻 Gabriel Santos Ramos (_gsramos)
- 👨‍💻 Júlio César de Carvalho (cesarsoft)
- 👨‍💻 Marco Antonio Araujo (_marcoaz)
- 👩‍💻 Yasmim Muniz Da Silva Caraça (yasmimcaraca)

