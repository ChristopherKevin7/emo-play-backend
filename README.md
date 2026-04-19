# EMO-Play Backend

Backend .NET 8 para o projeto EMO-Play — plataforma gamificada de reconhecimento e expressão de emoções para crianças com TEA.

---

## 🏗️ Arquitetura

O projeto segue **Clean Architecture** com quatro camadas:

| Camada | Responsabilidade |
|---|---|
| `EMOPlay.Domain` | Entidades, enums e interfaces de repositório |
| `EMOPlay.Application` | DTOs, serviços e interfaces de aplicação |
| `EMOPlay.Infrastructure` | MongoDB, repositórios, serviço de IA |
| `EMOPlay.API` | Controllers HTTP, middlewares, configuração |

---

## 🔧 Pré-requisitos

- **.NET 8.0** ou superior
- **MongoDB** 4.0+ (local ou Atlas)
- **Módulo de IA Python** (FastAPI, porta 8000)

---

## ⚙️ Configuração

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
    "Url": "http://localhost:8000"
  },
  "JwtSettings": {
    "Secret": "<sua-chave-secreta>",
    "ExpirationHours": 24
  }
}
```

---

## 🚀 Executando

```bash
dotnet restore
cd EMOPlay.API
dotnet run
```

- API: **http://localhost:5000**
- Swagger: **http://localhost:5000/swagger**
- Logs: `EMOPlay.API/logs/emoplay-YYYYMMDD.txt`

---

## 🗄️ Banco de Dados (MongoDB)

Coleções criadas automaticamente:

| Coleção | Conteúdo |
|---|---|
| `Users` | Psicólogos e crianças (role-based) |
| `GameSessions` | Sessões do Desafio 1 |
| `Challenges` | Respostas individuais do Desafio 1 |
| `SessionResults` | Resultados consolidados do Desafio 2 (batch-analyze) |

---

## 🔌 Endpoints

### Auth
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/auth/register` | Cria novo usuário |
| POST | `/api/auth/login` | Autentica e retorna JWT |

### Games (Desafio 1 — Identificar emoções)
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/games/start` | Inicia sessão de jogo |
| POST | `/api/games/end` | Finaliza sessão |
| POST | `/api/games/results` | Salva resultado consolidado |
| GET | `/api/games/results/{sessionId}` | Recupera resultado de uma sessão |

### Challenges (Desafio 1)
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/challenges/record-response` | Registra resposta da criança a um desafio |

### Emotions (Desafio 2 — Expressar emoções)
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/emotions/analyze` | Analisa emoção de uma única imagem |
| POST | `/api/emotions/batch-analyze` | Analisa múltiplas emoções com múltiplos frames |

### Metrics
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/metrics/child/{childId}` | Retorna métricas agregadas da criança |

---

## 🎮 Desafio 2 — Batch Analyze

### Payload de entrada
```json
{
  "userId": "uuid",
  "sessionId": "uuid (opcional — gerado automaticamente se omitido)",
  "attempts": [
    {
      "targetEmotion": "happy",
      "images": ["base64_frame1", "base64_frame2", "base64_frame3"],
      "responseTime": 5200
    }
  ]
}
```

### Resposta
```json
{
  "sessionId": "uuid",
  "total": 6,
  "correct": 4,
  "accuracy": 0.66,
  "processedAt": "2026-04-19T17:00:00Z",
  "message": "Batch analysis concluído: 4/6 corretas (66.67%)",
  "results": [
    {
      "targetEmotion": "happy",
      "isCorrect": true,
      "responseTime": 5200,
      "topPredictions": [
        { "emotion": "happy", "score": 0.72 },
        { "emotion": "surprise", "score": 0.18 },
        { "emotion": "neutral", "score": 0.10 }
      ],
      "averageScores": { "happy": 0.72, "surprise": 0.18, "neutral": 0.10 },
      "analysisTimestamp": "2026-04-19T17:00:01Z"
    }
  ]
}
```

### Regras de avaliação
Uma emoção é considerada **correta** se:
- A emoção alvo está no **TOP 2** das predictions, **OU**
- O score médio da emoção alvo for **> 0.4**

---

## 🤖 Integração com Módulo de IA

O `AIService` faz `POST http://localhost:8000/api/v1/analyze`.

**Payload enviado:**
```json
{
  "images": ["base64_frame1", "base64_frame2", "base64_frame3"]
}
```

**Resposta esperada:**
```json
{
  "predictions": [
    { "emotion": "happy", "score": 0.72 },
    { "emotion": "surprise", "score": 0.18 },
    { "emotion": "neutral", "score": 0.10 }
  ]
}
```

---

## 📊 Métricas — `GET /api/metrics/child/{childId}`

### Query params opcionais
- `sessionId` — filtra por sessão específica
- `startDate` / `endDate` — filtra por período (`YYYY-MM-DD`)

### Estrutura de resposta
```json
{
  "childId": "uuid",
  "childName": "João Silva",
  "desafio_1": {
    "totalSessions": 5,
    "accuracyRate": 0.78,
    "averageResponseTimeMs": 3200,
    "emotionBreakdown": {
      "Happy": { "attempts": 8, "correctAttempts": 7, "accuracy": 0.875 }
    },
    "progressTrend": [{ "date": "2026-04-19", "accuracy": 0.78 }],
    "historicAttempts": [
      { "date": "2026-04-19T10:00:00Z", "accuracyRate": 0.80, "level": "easy" }
    ]
  },
  "desafio_2": {
    "totalSessions": 3,
    "accuracyRate": 0.66,
    "averageResponseTimeMs": 4500,
    "emotionBreakdown": {
      "happy": {
        "attempts": 3,
        "correctAttempts": 2,
        "accuracy": 0.66,
        "avgTargetScore": 0.61,
        "avgResponseTimeMs": 5200
      }
    },
    "progressTrend": [{ "date": "2026-04-19", "accuracy": 0.66 }],
    "historicAttempts": [
      {
        "date": "2026-04-19T17:00:00Z",
        "accuracyRate": 0.66,
        "details": [
          {
            "targetEmotion": "happy",
            "isCorrect": true,
            "responseTimeMs": 5200,
            "topPredictions": [
              { "emotion": "happy", "score": 0.72 },
              { "emotion": "surprise", "score": 0.18 },
              { "emotion": "neutral", "score": 0.10 }
            ]
          }
        ]
      }
    ]
  },
  "disclaimer": "These metrics are for educational purposes and do not replace professional evaluation."
}
```

---

## 🔒 Segurança

- ✅ Autenticação JWT (HS256) com roles (`Psychologist = 0`, `Child = 1`)
- ✅ CORS configurado para `http://localhost:5173` e `http://localhost:3000`
- ✅ Logging estruturado com Serilog

---

## 📚 Dependências Principais

| Pacote | Versão | Uso |
|---|---|---|
| `MongoDB.Driver` | 2.25.0 | Banco de dados |
| `Serilog.AspNetCore` | 8.x | Logging |
| `AutoMapper` | 13.0.1 | Mapeamento de objetos |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.x | Autenticação JWT |

---

## 📋 Status do Projeto

| Feature | Status |
|---|---|
| Clean Architecture | ✅ |
| MongoDB | ✅ |
| Autenticação JWT | ✅ |
| Desafio 1 (identificar emoções) | ✅ |
| Desafio 2 (expressar emoções, multi-frame) | ✅ |
| Integração com módulo de IA Python | ✅ |
| Métricas por criança (Desafio 1 e 2) | ✅ |
| Testes unitários | 🔄 |
| Transações MongoDB | 🔄 (requer replica set) |
