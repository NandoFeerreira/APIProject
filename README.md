# APIProject

## English
A .NET Core API project built using Clean Architecture principles, implementing CQRS pattern with MediatR.

### Project Structure
- **APIProject.API**: API layer containing controllers and configuration
- **APIProject.Application**: Application layer with business logic and CQRS implementations
- **APIProject.Domain**: Core domain layer with entities and business rules
- **APIProject.Infrastructure**: Infrastructure layer handling external concerns

### Technologies & Patterns
- .NET Core
- Clean Architecture
- CQRS with MediatR
- JWT Authentication
- Entity Framework Core
- FluentValidation

### Getting Started
1. Clone the repository
2. Restore NuGet packages
3. Update the database using the script: `scripts\check-migrations.ps1`
4. Build and run the project

### Database Migrations
The project includes scripts to help manage database migrations:

- `scripts\check-migrations.ps1`: Checks for pending migrations and applies them if needed
- `scripts\update-database.ps1`: Checks for model changes, creates new migrations, and applies them

You can also use standard Entity Framework commands:

```powershell
# Create a new migration
dotnet ef migrations add MigrationName --project .\APIProject.Infrastructure --startup-project .\APIProject.API

# Apply migrations to the database
dotnet ef database update --project .\APIProject.Infrastructure --startup-project .\APIProject.API
```

---

## Português
Uma API .NET Core construída utilizando os princípios da Clean Architecture, implementando o padrão CQRS com MediatR.

### Estrutura do Projeto
- **APIProject.API**: Camada da API contendo controllers e configurações
- **APIProject.Application**: Camada de aplicação com lógica de negócios e implementações CQRS
- **APIProject.Domain**: Camada de domínio com entidades e regras de negócio
- **APIProject.Infrastructure**: Camada de infraestrutura lidando com preocupações externas

### Tecnologias & Padrões
- .NET Core
- Clean Architecture
- CQRS com MediatR
- Autenticação JWT
- Entity Framework Core
- FluentValidation

### Como Começar
1. Clone o repositório
2. Restaure os pacotes NuGet
3. Atualize o banco de dados usando o script: `scripts\check-migrations.ps1`
4. Compile e execute o projeto

### Migrações de Banco de Dados
O projeto inclui scripts para ajudar a gerenciar migrações de banco de dados:

- `scripts\check-migrations.ps1`: Verifica migrações pendentes e as aplica se necessário
- `scripts\update-database.ps1`: Verifica mudanças no modelo, cria novas migrações e as aplica

Você também pode usar os comandos padrão do Entity Framework:

```powershell
# Criar uma nova migração
dotnet ef migrations add NomeDaMigracao --project .\APIProject.Infrastructure --startup-project .\APIProject.API

# Aplicar migrações ao banco de dados
dotnet ef database update --project .\APIProject.Infrastructure --startup-project .\APIProject.API
```