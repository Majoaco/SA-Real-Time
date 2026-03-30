# SA Real-Time - Speech Analytics en Tiempo Real

Sistema de asistencia en tiempo real para agentes de call center BBVA. Analiza la llamada mientras transcurre y envía sugerencias al empleado basándose en el protocolo de calidad BBVA.

## Contexto del Proyecto

Este proyecto nace como evolución del sistema de Speech Analytics post-call existente (Python/Streamlit). La diferencia fundamental es que este sistema opera **durante la llamada**, no después.

**Repositorio anterior (post-call):** Análisis batch/individual de audios ya grabados.
**Este repositorio (real-time):** Transcripción streaming + sugerencias en vivo al agente.

## Stack Tecnológico

| Componente | Tecnología |
|-----------|-----------|
| Backend API | .NET 8 (ASP.NET Core Web API) |
| Real-time Communication | SignalR (WebSocket) |
| Frontend | Angular 18 |
| LLM | Azure OpenAI (GPT-4o) |
| Transcripción (futuro) | Azure Speech-to-Text Streaming |
| Tests | xUnit + FluentAssertions + NSubstitute |

## Arquitectura

Clean Architecture con 4 capas + TDD:

```
┌─────────────────────────────────────────────┐
│              Angular Client                  │
│  (SignalR client, live-call dashboard)       │
└──────────────────┬──────────────────────────┘
                   │ WebSocket (SignalR)
┌──────────────────▼──────────────────────────┐
│          WebAPI (ASP.NET Core)               │
│  ├── CallSessionHub (SignalR Hub)            │
│  ├── CallSessionController (REST)            │
│  └── SignalRNotifier (push to clients)       │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│          Application Layer                   │
│  ├── LiveCallOrchestrator (caso de uso)      │
│  ├── Interfaces (contratos)                  │
│  │   ├── ITranscriptionService               │
│  │   ├── ILlmAnalysisService                 │
│  │   ├── ICallSessionRepository              │
│  │   └── IRealTimeNotifier                   │
│  └── DTOs                                    │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│          Infrastructure Layer                │
│  ├── AzureOpenAiAnalysisService              │
│  ├── InMemoryCallSessionRepository           │
│  └── (futuro: AzureSpeechTranscription)      │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│          Domain Layer                        │
│  ├── Entities                                │
│  │   ├── LiveCallSession (agregado raíz)     │
│  │   ├── EvaluationSection                   │
│  │   ├── EvaluationItem                      │
│  │   └── CriticalError                       │
│  ├── Value Objects                           │
│  │   ├── Score                               │
│  │   ├── TemperatureReading                  │
│  │   ├── TranscriptSegment                   │
│  │   └── Suggestion                          │
│  └── Enums                                   │
│      ├── CallType (Inbound/Outbound)         │
│      ├── CallStatus (Idle→InProgress→Done)   │
│      ├── EvaluationResult (Yes/No/Exempt)    │
│      └── TemperatureLevel (1-5)              │
└─────────────────────────────────────────────┘
```

### Flujo de Datos Real-Time

```
Teléfono/Navegador
    │ audio stream
    ▼
Transcripción Streaming (Azure Speech-to-Text)
    │ texto parcial cada ~1-3s
    ▼
SignalR Hub (WebSocket)
    │
    ├──► Buffer de contexto (transcripción acumulada)
    │
    ├──► Cada ~15-30s → LLM (GPT-4o)
    │         ├── Sugerencias para el agente
    │         ├── Pasos del protocolo completados
    │         └── Temperatura (emocional/venta/conflicto)
    │
    └──► Push al frontend del agente
              ├── Transcripción en vivo
              ├── Sugerencias (máx 2, accionables)
              ├── Semáforo de temperatura
              └── Checklist del protocolo BBVA
```

## Métricas de Calidad BBVA (Reutilizadas del proyecto anterior)

El sistema evalúa 10 secciones del protocolo BBVA con un total de 100 puntos:

| # | Sección | Pts | SLA | Descripción |
|---|---------|-----|-----|-------------|
| 1 | APERTURA | 10 | Si | Presentación, origen, persona, motivo |
| 2 | DISCURSO | 4 | No | Mensaje comercial claro y respetuoso |
| 3 | CORDIALIDAD | 4 | No | Trato agradable, empático y positivo |
| 4 | CIERRES PARCIALES | 4 | No | Obtiene compromisos parciales del cliente |
| 5 | CONDUCCIÓN | 4 | No | Guía la llamada con seguridad |
| 6 | PROCEDIMIENTO/BENEFICIOS | 20 | Si | Seguimiento del speech, info correcta |
| 7 | OBJECIONES | 20 | No | Rebate objeciones con argumentos |
| 8 | SOLICITUD DATOS | 20 | Si | Datos para venta, ECU, alertas SMS |
| 9 | DESPEDIDA | 10 | Si | Cierre formal, rellamado, condiciones |
| 10 | REGISTRO SISTEMA | 4 | No | Codificación correcta |

**SLA = 60 puntos** (secciones 1, 6, 8, 9 = críticas para compliance)

### Errores Críticos (anulan puntaje)

| Error | Descripción |
|-------|-------------|
| Incentiva baja del servicio | Incentiva a la baja o a no entregar datos |
| Miente al cliente | Info gravemente incorrecta que perjudique |
| No muestra respeto | Falta de respeto al usuario |
| No obtiene SI explícito | Sin aceptación explícita (exento si rechaza) |
| Continúa sin requisitos | Venta sin que el cliente cumpla requisitos |

### Métricas de Temperatura (3 dimensiones, 0-100)

| Dimensión | Escala | Uso en real-time |
|-----------|--------|-----------------|
| **Emocional** | 0-20 Muy negativo, 21-40 Negativo, 41-60 Neutro, 61-80 Positivo, 81-100 Muy positivo | Semáforo de sentimiento |
| **Venta** | 0-20 Muy frío, 21-40 Frío, 41-60 Tibio, 61-80 Caliente, 81-100 Muy caliente | Indicador de interés del cliente |
| **Conflicto** | 0-20 Sin conflicto, 21-40 Tensión leve, 41-60 Moderada, 61-80 Conflicto, 81-100 Severo | Alerta si > 60 |

## Estructura del Proyecto

```
SA-Real-Time/
├── SpeechAnalyticsRealTime.sln
│
├── src/
│   ├── SpeechAnalytics.Domain/           # Entidades, Value Objects, Enums (0 dependencias)
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Enums/
│   │
│   ├── SpeechAnalytics.Application/      # Casos de uso, interfaces, DTOs (depende de Domain)
│   │   ├── Services/
│   │   ├── Interfaces/
│   │   └── DTOs/
│   │
│   ├── SpeechAnalytics.Infrastructure/   # Implementaciones externas (depende de Application)
│   │   ├── Persistence/
│   │   ├── LlmAnalysis/
│   │   └── Transcription/ (pendiente)
│   │
│   └── SpeechAnalytics.WebAPI/           # Host, controllers, SignalR (depende de Infra + App)
│       ├── Controllers/
│       └── Hubs/
│
├── tests/
│   ├── SpeechAnalytics.Domain.Tests/     # 32 tests (value objects, entities)
│   └── SpeechAnalytics.Application.Tests/ # 8 tests (orchestrator con mocks)
│
└── client/                                # Angular 18 app
    └── src/app/
        ├── core/services/                 # SignalR service
        ├── core/models/                   # TypeScript interfaces
        └── features/live-call/            # Componente principal
```

## Tests (40 total)

| Proyecto | Tests | Cubre |
|---------|-------|-------|
| Domain.Tests | 32 | Score (12), TemperatureReading (9), LiveCallSession (11) |
| Application.Tests | 8 | LiveCallOrchestrator: start, transcript, suggestions, temperature, end |

### Correr tests

```bash
dotnet test
```

## Setup

### Requisitos

- .NET 8 SDK
- Node.js 18+ y npm (para Angular)
- Cuenta Azure OpenAI con deployment GPT-4o

### Backend

```bash
# Clonar
git clone https://github.com/Majoaco/SA-Real-Time.git
cd SA-Real-Time

# Configurar Azure OpenAI en appsettings.json
# Editar src/SpeechAnalytics.WebAPI/appsettings.json

# Build y tests
dotnet build
dotnet test

# Correr API
cd src/SpeechAnalytics.WebAPI
dotnet run
# API en http://localhost:5000
# SignalR Hub en ws://localhost:5000/hubs/call-session
# Swagger en http://localhost:5000/swagger
```

### Frontend

```bash
cd client
npm install
npm start
# App en http://localhost:4200
```

## API Endpoints

### REST

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/callsession/start` | Inicia una sesión de llamada |
| POST | `/api/callsession/{id}/end` | Finaliza una sesión |
| GET | `/api/callsession/active` | Lista sesiones activas |
| GET | `/api/callsession/{id}` | Detalle de una sesión |

### SignalR Hub (`/hubs/call-session`)

| Método (Client -> Server) | Params | Descripción |
|--------------------------|--------|-------------|
| `StartCall` | agentId, callType | Inicia llamada |
| `SendTranscriptChunk` | sessionId, speaker, text, timestamp | Envía fragmento de transcripción |
| `RequestSuggestions` | sessionId | Solicita sugerencias al LLM |
| `RequestTemperature` | sessionId | Solicita análisis de temperatura |
| `EndCall` | sessionId | Finaliza llamada |
| `JoinSession` | sessionId | Supervisor se une a sesión |

| Evento (Server -> Client) | Payload | Descripción |
|--------------------------|---------|-------------|
| `CallStarted` | {sessionId, status} | Llamada iniciada |
| `TranscriptUpdate` | {speaker, text, timestamp} | Nuevo segmento de transcripción |
| `NewSuggestion` | {text, category, priority} | Sugerencia para el agente |
| `TemperatureUpdate` | {emotional, sales, conflict, labels} | Temperatura actualizada |
| `ChecklistUpdate` | {completedSteps, currentPhase} | Progreso del protocolo |
| `SessionStatus` | string | Cambio de estado de sesión |

## Principios de Desarrollo

- **TDD**: Tests primero, implementación después
- **Clean Architecture**: Domain sin dependencias, flujo de dependencias hacia adentro
- **Clean Code**: Nombres descriptivos, responsabilidad única, sin código muerto
- **Domain-Driven Design**: Entidades ricas, value objects inmutables, agregados con invariantes

## Roadmap

- [x] Domain con TDD (entidades, value objects, enums)
- [x] Application con TDD (orquestador, interfaces, DTOs)
- [x] Infrastructure (Azure OpenAI, repositorio in-memory)
- [x] WebAPI con SignalR Hub
- [x] Angular client (estructura, SignalR service, live-call component)
- [ ] Instalar Node.js y completar setup Angular (`npm install` + `ng serve`)
- [ ] Integrar Azure Speech-to-Text streaming (transcripción real desde audio)
- [ ] Captura de audio desde navegador (getUserMedia API)
- [ ] Dashboard de supervisor (ver llamadas activas en tiempo real)
- [ ] Persistencia en PostgreSQL (migrar de InMemoryRepository)
- [ ] Autenticación JWT
- [ ] Evaluación completa de calidad BBVA al finalizar llamada
- [ ] Anonimización de datos sensibles (CI, teléfono, email, tarjeta)
