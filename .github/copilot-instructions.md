# EMO-Play Backend - Instruções para Desenvolvimento

## 📌 Visão Geral

Este backend .NET é responsável por **intermediar a comunicação entre o Frontend (React + Vite) e o Módulo de IA (Python)** de reconhecimento de emoções. O backend atua como orquestrador, recebendo dados do frontend, acionando o módulo de IA e gerenciando métricas e relatórios para o psicólogo.

## 🎯 Responsabilidades Principais

O backend deve implementar os seguintes fluxos:

1. **Receber e armazenar dados do jogo** (acertos/erros das crianças)
2. **Processar imagens da webcam** e enviar para o módulo de IA
3. **Retornar análises de emoção** em tempo real
4. **Calcular e retornar métricas** para relatórios do psicólogo

---

## 🔌 Endpoints Obrigatórios

### 1. **POST /api/games/start**
Inicia uma nova sessão de jogo.

**Request:**
```json
{
  "childId": "uuid",
  "gameMode": "identify-emotion" | "make-emotion",
  "psychologistId": "uuid"
}
```

**Response:**
```json
{
  "sessionId": "uuid",
  "gameMode": "identify-emotion",
  "status": "started",
  "timestamp": "2026-03-29T10:30:00Z"
}
```

---

### 2. **POST /api/emotions/analyze**
Recebe imagem da webcam e solicita análise ao módulo de IA.

**Request:**
```json
{
  "sessionId": "uuid",
  "image": "base64-encoded-image",
  "targetEmotion": "happy" | "sad" | "angry" | "surprised" | "fearful" | "disgusted" | "neutral",
  "challengeId": "uuid"
}
```

**Response:**
```json
{
  "detectedEmotion": "happy",
  "confidence": 0.92,
  "isCorrect": true,
  "analysisTimestamp": "2026-03-29T10:30:15Z"
}
```

---

### 3. **POST /api/games/record-response**
Registra a resposta da criança (acerto ou erro).

**Request:**
```json
{
  "sessionId": "uuid",
  "challengeId": "uuid",
  "targetEmotion": "happy",
  "childResponse": "happy" | "sad" | "angry" | "surprised" | "fearful" | "disgusted" | "neutral",
  "isCorrect": true | false,
  "responseTime": 5000,
  "confidence": 0.92,
  "imageUrl": "path-or-url-to-captured-image"
}
```

**Response:**
```json
{
  "recorded": true,
  "points": 10,
  "totalPoints": 45,
  "message": "Resposta registrada com sucesso"
}
```

---

### 4. **GET /api/metrics/child/:childId**
Retorna as métricas da criança para o relatório do psicólogo.

**Query Parameters:**
- `sessionId` (opcional): UUID da sessão específica
- `startDate` (opcional): Data de início (YYYY-MM-DD)
- `endDate` (opcional): Data de fim (YYYY-MM-DD)

**Response:**
```json
{
  "childId": "uuid",
  "childName": "João Silva",
  "totalSessions": 5,
  "accuracyRate": 0.78,
  "averageResponseTime": 3200,
  "averageConfidence": 0.85,
  "emotionBreakdown": {
    "happy": {
      "attempts": 8,
      "correctAttempts": 7,
      "accuracy": 0.875,
      "avgConfidence": 0.88
    },
    "sad": {
      "attempts": 6,
      "correctAttempts": 4,
      "accuracy": 0.667,
      "avgConfidence": 0.82
    },
    "angry": {
      "attempts": 5,
      "correctAttempts": 3,
      "accuracy": 0.6,
      "avgConfidence": 0.79
    }
  },
  "progressTrend": [
    { "date": "2026-03-25", "accuracy": 0.65 },
    { "date": "2026-03-26", "accuracy": 0.72 },
    { "date": "2026-03-27", "accuracy": 0.78 }
  ],
  "disclaimer": "Essas métricas são para fins educacionais e não substituem avaliação profissional."
}
```

---

### 5. **POST /api/games/end**
Finaliza uma sessão de jogo.

**Request:**
```json
{
  "sessionId": "uuid",
  "totalPoints": 45,
  "totalChallenges": 5,
  "accuracyRate": 0.8
}
```

**Response:**
```json
{
  "sessionId": "uuid",
  "status": "completed",
  "summary": {
    "totalPoints": 45,
    "totalChallenges": 5,
    "accuracyRate": 0.8,
    "averageResponseTime": 3200
  },
  "message": "Sessão finalizada com sucesso"
}
```

---

## 🔄 Fluxo de Integração com Módulo de IA

1. **Frontend captura imagem** → Envia para `/api/emotions/analyze`
2. **Backend recebe imagem** → Envia para Módulo de IA (Python) via HTTP/gRPC
3. **Módulo de IA analisa** → Retorna emoção detectada + confiança
4. **Backend processa** → Valida resultado e retorna ao Frontend
5. **Frontend fornece feedback** → Criança recebe confirmação

---

## 💾 Modelos de Dados (Sugerido)

### Child
```csharp
public class Child
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int AgeRange { get; set; }
    public Guid PsychologistId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### GameSession
```csharp
public class GameSession
{
    public Guid Id { get; set; }
    public Guid ChildId { get; set; }
    public string GameMode { get; set; }
    public int TotalPoints { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<Challenge> Challenges { get; set; }
}
```

### Challenge
```csharp
public class Challenge
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string TargetEmotion { get; set; }
    public string ChildResponse { get; set; }
    public bool IsCorrect { get; set; }
    public int ResponseTime { get; set; }
    public double Confidence { get; set; }
    public string ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 🛢️ Requisitos de Banco de Dados

- **Persistência**: MongoDB (NoSQL)
- **Coleções principais**: Children, Psychologists, GameSessions, Challenges, SessionResults
- **Índices**: ChildId, SessionId para queries rápidas
- **Auditoria**: Rastrear todas as respostas por data/hora para relatórios

---

## 🔐 Considerações de Segurança

- ✅ Autenticação JWT para endpoints
- ✅ Validação de childId vs. psychologistId (autorização)
- ✅ CORS configurado para aceitar apenas Frontend
- ✅ Rate limiting para `/api/emotions/analyze` (proteção contra abuso)
- ✅ Validação de imagens antes de processar (formato, tamanho)
- ✅ Criptografia de dados sensíveis (como fotos)

---

## 📦 Stack Recomendado

- **Framework**: ASP.NET Core 8
- **Banco de Dados**: MongoDB
- **Driver MongoDB**: MongoDB.Driver
- **HTTP Client**: HttpClientFactory para comunicação com IA
- **Logging**: Serilog
- **Autenticação**: JWT (Microsoft.AspNetCore.Authentication.JwtBearer)

---

## 📝 Notas Importantes

1. **Módulo de IA**: Deve estar rodando em servidor separado (Python + Flask/FastAPI)
2. **Comunicação**: Backend fará chamadas HTTP para o Módulo de IA
3. **Imagens**: Armazenar de forma segura (blob storage ou filesystem seguro)
4. **Métricas**: Calcular em tempo real ou cache para performance
5. **Relatórios**: Dados devem suportar filtros por período para melhor análise

---

## 🎯 Próximos Passos

1. Estruturar projeto .NET com arquitetura em camadas
2. Configurar banco de dados com migrations
3. Implementar endpoints na ordem: start → record-response → end → metrics
4. Testar integração com Frontend React
5. Configurar comunicação com Módulo de IA Python
6. Implementar autenticação e segurança
7. Realizar testes de performance e segurança

