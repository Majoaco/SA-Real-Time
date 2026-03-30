import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import {
  TranscriptUpdate,
  Suggestion,
  TemperatureUpdate,
  ChecklistUpdate,
  StartCallResponse
} from '../models/call-session.model';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection!: signalR.HubConnection;

  // Observables for real-time events
  readonly callStarted$ = new Subject<StartCallResponse>();
  readonly transcriptUpdate$ = new Subject<TranscriptUpdate>();
  readonly newSuggestion$ = new Subject<Suggestion>();
  readonly temperatureUpdate$ = new Subject<TemperatureUpdate>();
  readonly checklistUpdate$ = new Subject<ChecklistUpdate>();
  readonly sessionStatus$ = new Subject<string>();

  async connect(): Promise<void> {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl)
      .withAutomaticReconnect()
      .build();

    this.registerHandlers();
    await this.hubConnection.start();
  }

  async disconnect(): Promise<void> {
    await this.hubConnection?.stop();
  }

  async startCall(agentId: string, callType: string): Promise<void> {
    await this.hubConnection.invoke('StartCall', agentId, callType);
  }

  async sendTranscriptChunk(sessionId: string, speaker: string, text: string, timestampSeconds: number): Promise<void> {
    await this.hubConnection.invoke('SendTranscriptChunk', sessionId, speaker, text, timestampSeconds);
  }

  async requestSuggestions(sessionId: string): Promise<void> {
    await this.hubConnection.invoke('RequestSuggestions', sessionId);
  }

  async requestTemperature(sessionId: string): Promise<void> {
    await this.hubConnection.invoke('RequestTemperature', sessionId);
  }

  async endCall(sessionId: string): Promise<void> {
    await this.hubConnection.invoke('EndCall', sessionId);
  }

  async joinSession(sessionId: string): Promise<void> {
    await this.hubConnection.invoke('JoinSession', sessionId);
  }

  private registerHandlers(): void {
    this.hubConnection.on('CallStarted', (data: StartCallResponse) => this.callStarted$.next(data));
    this.hubConnection.on('TranscriptUpdate', (data: TranscriptUpdate) => this.transcriptUpdate$.next(data));
    this.hubConnection.on('NewSuggestion', (data: Suggestion) => this.newSuggestion$.next(data));
    this.hubConnection.on('TemperatureUpdate', (data: TemperatureUpdate) => this.temperatureUpdate$.next(data));
    this.hubConnection.on('ChecklistUpdate', (data: ChecklistUpdate) => this.checklistUpdate$.next(data));
    this.hubConnection.on('SessionStatus', (data: string) => this.sessionStatus$.next(data));
  }
}
