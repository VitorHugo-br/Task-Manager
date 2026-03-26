# Task Manager

API REST para gerenciamento de tarefas, comentários, usuários, autenticação e cache, desenvolvida com **ASP.NET Core**, **Entity Framework Core**, **MySQL** e **Redis**.

## Tecnologias

- .NET 8
- ASP.NET Core MVC
- Entity Framework Core
- MySQL
- Redis
- JWT Authentication
- OpenAPI / Scalar / Swagger
- Docker e Docker Compose

## Funcionalidades

- Cadastro e login de usuários
- Autenticação com JWT
- Criação, edição e listagem de tarefas
- Filtros e paginação de tarefas
- Comentários em tarefas
- Cache de consultas com Redis
- Controle de acesso por autorização e papéis
- Logs de auditoria e sistema

## Estrutura geral

- `AuthController` — autenticação e registro
- `UserController` — listagem de usuários e issuers
- `MyTasksController` — operações com tarefas
- `CommentsController` — comentários
- `Data/TaskDbContext.cs` — contexto do banco
- `Services/` — serviços de autenticação, auditoria, logs e Redis
- `Extensions/` — extensões de configuração da aplicação
