export interface StartCallRequest {
  agentId: string;
  callType: 'Inbound' | 'Outbound';
}

export interface StartCallResponse {
  sessionId: string;
  status: string;
}

export interface TranscriptUpdate {
  speaker: string;
  text: string;
  timestampSeconds: number;
  isFinal: boolean;
}

export interface Suggestion {
  text: string;
  category: string;
  priority: 'Low' | 'Normal' | 'High' | 'Critical';
}

export interface TemperatureUpdate {
  emotional: number;
  sales: number;
  conflict: number;
  emotionalLabel: string;
  salesLabel: string;
  conflictLabel: string;
  requiresAttention: boolean;
}

export interface ChecklistUpdate {
  completedSteps: string[];
  currentPhase: string | null;
}

export interface CallSession {
  id: string;
  agentId: string;
  callType: string;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  duration: number;
  transcript: string;
  suggestions: Suggestion[];
  temperature: TemperatureUpdate | null;
  completedSteps: string[];
}
