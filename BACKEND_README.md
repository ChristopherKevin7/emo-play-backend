# EMO-Play Backend

Backend .NET para o projeto EMO-Play - Plataforma gamificada de reconhecimento de emoções para crianças com TEA.

## 🏗️ Estrutura do Projeto

O projeto segue a arquitetura **Clean Architecture** com as seguintes camadas:

### **EMOPlay.Domain**
- **Entities**: Modelos de negócio (Child, Psychologist, GameSession, Challenge)
- **Enums**: Enumerações (GameModeEnum, EmotionEnum, GameSessionStatusEnum)
- **Interfaces**: Contratos para repositórios e UnitOfWork

### **EMOPlay.Application**
- **DTOs**: Modelos de transferência de dados
- **Services**: Lógica de aplicação (GameService, ChallengeService, MetricsService)
- **Interfaces**: Contratos para serviços

### **EMOPlay.Infrastructure**
- **Data**: DbContext e configurações de banco de dados
- **Repositories**: Implementação do padrão Repository
- **ExternalServices**: Integração com módulo de IA (AIService)

### **EMOPlay.API**
- **Controllers**: Endpoints HTTP (GamesController, EmotionsController, ChallengesController, MetricsController)
- **Middleware**: Middlewares customizados
- **Extensions**: Extensões e configurações de extensão

---

## 🔧 Pré-requisitos

- **.NET 8.0** ou superior
- **MongoDB** 4.0 ou superior
- **Node.js/npm** (para o frontend, se desejado rodar junto)

---

## 📦 Instalação e Configuração

### 1. Clone o repositório
```bash
git clone <seu-repositorio>
cd backend
```

### 2. Atualize a string de conexão

Edite `EMOPlay.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MongoConnection": "mongodb://localhost:27017"
  },
  "MongoDBSettings": {
    "DatabaseName": "emoplay"
  },
  "AIService": {
    "Url": "http://localhost:5000"
  }
}
```

**Nota:** Se você está usando MongoDB Atlas (cloud), use a connection string fornecida:
```json
"MongoConnection": "mongodb+srv://usuario:senha@seu-cluster.mongodb.net/emoplay?retryWrites=true&w=majority"
```

### 3. Restaure os pacotes e execute

```bash
# Restaure os pacotes
dotnet restore

# Execute a aplicação
cd EMOPlay.API
dotnet run
```

A API estará disponível em: **http://localhost:5000**

Swagger UI: **http://localhost:5000/swagger**

---

## 🗄️ Banco de Dados (MongoDB)

O projeto utiliza **MongoDB** como banco de dados. As seguintes coleções são criadas automaticamente na inicialização:

- **Children**: Dados das crianças
- **Psychologists**: Dados dos psicólogos
- **GameSessions**: Sessões de jogo
- **Challenges**: Desafios individuais
- **SessionResults**: Resultados consolidados das sessões

### Índices Automáticos
Índices são criados automaticamente pelo MongoDbContext:
- `ChildId` em GameSessions, Challenges, SessionResults
- `SessionId` em Challenges, SessionResults
- `PsychologistId` em Children, GameSessions

---

## 🔌 Endpoints Principais

### Games
- `POST /api/games/start` - Inicia uma sessão de jogo
- `POST /api/games/end` - Finaliza uma sessão de jogo
- `POST /api/games/results` - Salva resultados consolidados de uma sessão
- `GET /api/games/results/{sessionId}` - Recupera resultados de uma sessão específica

### Emotions
- `POST /api/emotions/analyze` - Analisa emoção de uma imagem

### Challenges
- `POST /api/challenges/record-response` - Registra resposta da criança a um desafio

### Metrics
- `GET /api/metrics/child/{childId}` - Retorna métricas agregadas da criança

---



---

## 🤖 Integração com Módulo de IA

O serviço `AIService` (Infrastructure/ExternalServices/AIService.cs) faz requisições HTTP para o módulo de IA Python.

**Endpoint esperado do módulo de IA:**
```
POST http://localhost:5000/api/emotion/analyze
Content-Type: application/json

{
  "image": "base64-string",
  "targetEmotion": "happy"
}
```

**Resposta esperada:**
```json
{
  "detectedEmotion": "happy",
  "confidence": 0.92
}
```

---

## 🔒 Segurança

- ✅ CORS configurado para aceitar requisições do frontend (http://localhost:5173, http://localhost:3000)
- ✅ Validação de camadas de banco de dados
- ✅ Logging estruturado com Serilog
- 🔄 **TODO**: Autenticação JWT implementar
- 🔄 **TODO**: Rate limiting implementar

---

## 📝 Logging

Os logs são salvos em: `logs/emoplay-YYYYMMDD.txt`

Configurado via Serilog em `Program.cs`.

---

## 🧪 Testes

Testes unitários podem ser adicionados em uma pasta `tests/`:

```bash
dotnet new xunit -n EMOPlay.Tests
dotnet add tests/EMOPlay.Tests/EMOPlay.Tests.csproj reference EMOPlay.Domain/EMOPlay.Domain.csproj
```

---

## 📚 Dependências Principais

- **MongoDB.Driver 2.25.0** - Driver MongoDB
- **Serilog 3.1.1** - Logging estruturado
- **AutoMapper 13.0.1** - Mapeamento de objetos
- **Microsoft.AspNetCore.Authentication.JwtBearer** - Autenticação JWT (preparado)
- **Microsoft.Extensions.Configuration 10.0.5** - Configuração

---

## 🚀 Deploy

### Docker (Opcional)

Crie um `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "EMOPlay.API.dll"]
```

---

## 📋 Próximos Passos

1. ✅ Implementar estrutura base com Clean Architecture (CONCLUÍDO)
2. ✅ Migração para MongoDB (CONCLUÍDO)
3. ✅ Todos os endpoints principais implementados (CONCLUÍDO)
4. 🔄 Autenticação JWT
5. 🔄 Rate limiting
6. 🔄 Testes unitários
7. 🔄 Transações MongoDB (requer replica set)
8. 🔄 CI/CD pipeline
9. 🔄 Health checks

---

## 📄 Licença

Este projeto é parte do EMO-Play.

---

## 👨‍💻 Desenvolvedor

Estruturado seguindo Clean Architecture e melhores práticas .NET.

Para dúvidas, abra uma issue no repositório.
