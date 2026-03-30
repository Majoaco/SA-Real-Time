import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { SignalRService } from '../../core/services/signalr.service';
import {
  TranscriptUpdate,
  Suggestion,
  TemperatureUpdate,
} from '../../core/models/call-session.model';

@Component({
  selector: 'app-live-call',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './live-call.component.html',
  styleUrls: ['./live-call.component.css']
})
export class LiveCallComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  sessionId: string | null = null;
  agentId = '';
  callType: 'Inbound' | 'Outbound' = 'Outbound';
  isCallActive = false;

  transcriptSegments: TranscriptUpdate[] = [];
  suggestions: Suggestion[] = [];
  temperature: TemperatureUpdate | null = null;
  completedSteps: string[] = [];
  currentPhase: string | null = null;

  readonly protocolSteps = [
    { id: 'apertura', label: 'Apertura' },
    { id: 'discurso', label: 'Discurso' },
    { id: 'cordialidad', label: 'Cordialidad' },
    { id: 'cierres_parciales', label: 'Cierres Parciales' },
    { id: 'conduccion', label: 'Conduccion' },
    { id: 'procedimiento_beneficios', label: 'Procedimiento / Beneficios' },
    { id: 'aceptacion_objeciones', label: 'Objeciones' },
    { id: 'solicitud_datos', label: 'Solicitud Datos' },
    { id: 'despedida', label: 'Despedida' },
    { id: 'registro_sistema', label: 'Registro' }
  ];

  constructor(private signalR: SignalRService) {}

  ngOnInit(): void {
    this.signalR.connect();

    this.signalR.callStarted$.pipe(takeUntil(this.destroy$)).subscribe(response => {
      this.sessionId = response.sessionId;
      this.isCallActive = true;
    });

    this.signalR.transcriptUpdate$.pipe(takeUntil(this.destroy$)).subscribe(update => {
      this.transcriptSegments.push(update);
    });

    this.signalR.newSuggestion$.pipe(takeUntil(this.destroy$)).subscribe(suggestion => {
      this.suggestions.unshift(suggestion);
      if (this.suggestions.length > 5) this.suggestions.pop();
    });

    this.signalR.temperatureUpdate$.pipe(takeUntil(this.destroy$)).subscribe(temp => {
      this.temperature = temp;
    });

    this.signalR.checklistUpdate$.pipe(takeUntil(this.destroy$)).subscribe(update => {
      this.completedSteps = update.completedSteps;
      this.currentPhase = update.currentPhase;
    });

    this.signalR.sessionStatus$.pipe(takeUntil(this.destroy$)).subscribe(status => {
      if (status === 'Completed') {
        this.isCallActive = false;
      }
    });
  }

  async startCall(): Promise<void> {
    await this.signalR.startCall(this.agentId, this.callType);
  }

  async endCall(): Promise<void> {
    if (this.sessionId) {
      await this.signalR.endCall(this.sessionId);
    }
  }

  isStepCompleted(stepId: string): boolean {
    return this.completedSteps.includes(stepId);
  }

  getTemperatureColor(value: number): string {
    if (value <= 20) return '#dc3545';
    if (value <= 40) return '#fd7e14';
    if (value <= 60) return '#ffc107';
    if (value <= 80) return '#9acd32';
    return '#28a745';
  }

  getConflictColor(value: number): string {
    if (value <= 20) return '#28a745';
    if (value <= 40) return '#9acd32';
    if (value <= 60) return '#ffc107';
    if (value <= 80) return '#fd7e14';
    return '#dc3545';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalR.disconnect();
  }
}
