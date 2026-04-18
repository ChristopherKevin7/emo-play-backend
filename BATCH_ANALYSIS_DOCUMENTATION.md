# EMOPlay Backend - Implementação Batch Analysis

## Overview

Implementação completa do endpoint `POST /api/emotion/batch-analyze` que processa múltiplas imagens em batch via módulo de IA externo.

---

## Componentes Implementados

### 1. DTOs (Data Transfer Objects)

#### `BatchAttempt.cs`
- `TargetEmotion`: string - Emoção alvo
- `Image`: string (base64) - Imagem para análise

#### `BatchAnalyzeRequest.cs`
- `UserId`: string - ID do usuário
- `SessionId`: string - ID da sessão
- `Attempts`: List<BatchAttempt> - Array de tentativas

#### `BatchAnalyzeResult.cs`
- `TargetEmotion`: string - Emoção alvo
- `DetectedEmotion`: string - Emoção detectada pela IA
- `Confidence`: double - Confiança (0-1)
- `IsCorrect`: bool - Se atende aos critérios
- `AnalysisTimestamp`: DateTime - Momento da análise

#### `BatchAnalyzeResponse.cs`
- `SessionId`: string - ID da sessão processada
- `Total`: int - Total de tentativas
- `Correct`: int - Total de acertos
- `Accuracy`: double - Taxa de acerto (0-1)
- `Results`: List<BatchAnalyzeResult> - Resultados individuais
- `ProcessedAt`: DateTime - Timestamp do processamento
- `Message`: string - Mensagem descritiva

---

## 2. Service Layer

### `EmotionService.cs`

**Responsabilidades:**
- Validar entrada
- Processar múltiplas imagens em paralelo (até 3 concurrent)
- Chamar AIService para cada imagem
- Aplicar regras de negócio (confidence > 0.7)
- Salvar resultados no MongoDB
- Consolidar resposta final

**Métodos principais:**

```csharp
public async Task<BatchAnalyzeResponse> BatchAnalyzeAsync(BatchAnalyzeRequest request)
```
- Orquestra todo o fluxo de batch processing
- Processa até 3 requisições paralelas
- Retorna resposta consolidada

```csharp
private async Task<BatchAnalyzeResult> ProcessSingleAttemptAsync(BatchAttempt attempt, DateTime processedAt)
```
- Processa uma tentativa individual
- Chama AIService
- Aplica validações

```csharp
private async Task SaveBatchResultAsync(BatchAnalyzeRequest request, BatchAnalyzeResponse response)
```
- Persiste resultados no MongoDB
- Usa SessionResult existente para compatibilidade

---

## 3. Controller

### `EmotionsController.cs`

**Novo Endpoint:**
```
POST /api/emotion/batch-analyze
```

**Responsabilidades:**
- Validar entrada
- Injetar EmotionService
- Processar exceptões
- Retornar resposta adequada com status codes

**Validações implementadas:**
- SessionId obrigatório
- UserId obrigatório
- Array de tentativas não vazio
- Cada tentativa deve ter targetEmotion e image

---

## 4. Integração com MongoDB

Os resultados são salvos automaticamente através de `SessionResult`:

```csharp
var sessionResult = new SessionResult
{
    Id = Guid.NewGuid(),
    SessionId = Guid.Parse(request.SessionId),
    CorrectAnswers = response.Correct,
    TotalChallenges = response.Total,
    Percentage = response.Accuracy * 100,
    Message = response.Message,
    ResultsJson = JsonSerializer.Serialize(response.Results),
    CreatedAt = DateTime.UtcNow
};
```

---

## Exemplo de Uso

### Request

```bash
curl -X POST http://localhost:5000/api/emotion/batch-analyze \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "sessionId": "7a8b9c0d-e1f2-4a5b-8c9d-0e1f2a3b4c5d",
    "attempts": [
      {
        "targetEmotion": "happy",
        "image": "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9h..."
      },
      {
        "targetEmotion": "sad",
        "image": "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9h..."
      },
      {
        "targetEmotion": "angry",
        "image": "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9h..."
      }
    ]
  }'
```

### Response (200 OK)

```json
{
  "sessionId": "7a8b9c0d-e1f2-4a5b-8c9d-0e1f2a3b4c5d",
  "total": 3,
  "correct": 2,
  "accuracy": 0.6666666666666666,
  "results": [
    {
      "targetEmotion": "happy",
      "detectedEmotion": "happy",
      "confidence": 0.92,
      "isCorrect": true,
      "analysisTimestamp": "2026-04-11T15:30:45.123Z"
    },
    {
      "targetEmotion": "sad",
      "detectedEmotion": "sad",
      "confidence": 0.78,
      "isCorrect": true,
      "analysisTimestamp": "2026-04-11T15:30:46.456Z"
    },
    {
      "targetEmotion": "angry",
      "detectedEmotion": "fearful",
      "confidence": 0.65,
      "isCorrect": false,
      "analysisTimestamp": "2026-04-11T15:30:47.789Z"
    }
  ],
  "processedAt": "2026-04-11T15:30:47.789Z",
  "message": "Batch analysis concluído: 2/3 tentativas corretas (66.67%)"
}
```

---

## Fluxo de Processamento

```
1. Frontend > Backend
   ├─ POST /api/emotion/batch-analyze
   ├─ Payload com múltiplas tentativas
   └─ Cada tentativa: targetEmotion + base64 image

2. Controller
   ├─ Validar entrada
   ├─ Injetar EmotionService
   └─ Chamar BatchAnalyzeAsync()

3. EmotionService
   ├─ Para cada tentativa (paralelo, max 3):
   │  ├─ Chamar AIService.AnalyzeEmotionAsync()
   │  ├─ AIService > FastAPI (localhost:8000/analyze)
   │  ├─ Receber: emotion, confidence
   │  ├─ Comparar: targetEmotion vs detectedEmotion
   │  ├─ Aplicar regra: Correct se (emoção == target AND confidence > 0.7)
   │  └─ Montar BatchAnalyzeResult
   ├─ Consolidar resultados
   ├─ Salvar no MongoDB (SessionResult)
   └─ Retornar BatchAnalyzeResponse

4. Backend > Frontend
   ├─ Response 200 OK
   ├─ Estatísticas consolidadas
   └─ Resultados individuais
```

---

## Regras de Negócio

### Confidenfe Threshold
- **Valor mínimo**: 0.7 (70%)
- Localizado em: `EmotionService.cs` > `const double ConfidenceThreshold = 0.7`
- Fácil de ajustar conforme necessário

### Processamento Paralelo
- **Concorrência máxima**: 3 requisições simultâneas
- Implementado com `SemaphoreSlim`
- Evita sobrecarregar o módulo de IA

### Tratamento de Erros
- Falha em uma tentativa não afeta as demais
- Log detalhado de cada tentativa
- Resposta consolidada mesmo com parciais erros

---

## Configuração Necessária

### 1. Program.cs (Já adicionado)
```csharp
builder.Services.AddScoped<IEmotionService, EmotionService>();
```

### 2. AIService (Já existente)
Arquivo: `EMOPlay.Infrastructure\ExternalServices\AIService.cs`
- Já configurado para chamar módulo de IA
- URL padrão: `http://localhost:5000` (configurável via appsettings.json)

### 3. appsettings.json
```json
{
  "AIService": {
    "Url": "http://localhost:8000"
  }
}
```

---

## Logging

Todos os eventos são registrados com Serilog:

```
[INF] Iniciando batch analysis para sessão 7a8b9c0d-e1f2-4a5b-8c9d-0e1f2a3b4c5d com 3 tentativas
[INF] Processando tentativa para emoção alvo: happy
[INF] Resultado: CORRETO (Target: happy, Detectado: happy, Confiança: 92.00%)
[INF] Resultados salvos no MongoDB para sessão 7a8b9c0d-e1f2-4a5b-8c9d-0e1f2a3b4c5d
[INF] Batch analysis concluído: Batch analysis concluído: 2/3 tentativas corretas (66.67%)
```

---

## Expansão Futura

Para adicionar novos comportamentos:

1. **Novos campos de resultado**:
   - Editar `BatchAnalyzeResult.cs`
   - Atualizar lógica em `ProcessSingleAttemptAsync()`

2. **Novos critérios de acerto**:
   - Modificar logica em `ProcessSingleAttemptAsync()`
   - Exemplo: adicionar múltiplas emoções válidas por tentativa

3. **Novo armazenamento**:
   - Criar entidade específica para `BatchAnalysisResult`
   - Atualizar `SaveBatchResultAsync()` para persistir em nova coleção

4. **Diferentes módulos de IA**:
   - Estender `IAIService` com novos métodos
   - Implementar diferentes estratégias em `EmotionService`

---

## Status Atual

✅ **Implementação Completa**
- Todos os componentes funcionais
- Build sem erros
- Pronto para testes de integração

📦 **Arquivos criados/modificados:**
- ✅ DTOs/Emotion/BatchAttempt.cs
- ✅ DTOs/Emotion/BatchAnalyzeRequest.cs
- ✅ DTOs/Emotion/BatchAnalyzeResult.cs
- ✅ DTOs/Emotion/BatchAnalyzeResponse.cs
- ✅ Application/Services/EmotionService.cs
- ✅ Application/Interfaces/IEmotionService.cs
- ✅ API/Controllers/EmotionsController.cs (atualizado)
- ✅ Program.cs (atualizado)
- ✅ EMOPlay.Application.csproj (atualizado)
