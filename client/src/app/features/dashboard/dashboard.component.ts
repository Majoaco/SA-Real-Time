import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { environment } from '../../../environments/environment';

interface ActiveSession {
  id: string;
  agentId: string;
  callType: string;
  status: string;
  startedAt: string;
  duration: number;
  segmentCount: number;
  hasTemperature: boolean;
  checklistProgress: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="dashboard">
      <header class="dash-header">
        <h1>SA Real-Time - Dashboard</h1>
        <a routerLink="/live-call" class="btn-new-call">Nueva Llamada</a>
      </header>

      <section class="active-sessions">
        <h2>Sesiones Activas</h2>
        @if (sessions.length === 0) {
          <p class="empty">No hay sesiones activas</p>
        }
        <div class="sessions-grid">
          @for (session of sessions; track session.id) {
            <div class="session-card">
              <div class="card-header">
                <span class="badge" [class.active]="session.status === 'InProgress'">{{ session.status }}</span>
                <span class="call-type">{{ session.callType }}</span>
              </div>
              <p class="agent">Agente: {{ session.agentId }}</p>
              <p class="duration">Duracion: {{ formatDuration(session.duration) }}</p>
              <p class="segments">Segmentos: {{ session.segmentCount }}</p>
              <p class="checklist">Checklist: {{ session.checklistProgress }}/10</p>
            </div>
          }
        </div>
      </section>
    </div>
  `,
  styles: [`
    .dashboard { max-width: 1200px; margin: 0 auto; padding: 16px; font-family: 'Segoe UI', sans-serif; }
    .dash-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .dash-header h1 { font-size: 1.4rem; color: #1a1a2e; }
    .btn-new-call {
      padding: 10px 20px; background: #0f3460; color: white;
      border-radius: 6px; text-decoration: none; font-weight: 600;
    }
    .btn-new-call:hover { background: #1a5276; }
    .active-sessions h2 { font-size: 1.1rem; margin-bottom: 12px; }
    .empty { color: #999; font-style: italic; }
    .sessions-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 12px; }
    .session-card {
      background: white; border-radius: 8px; padding: 16px;
      box-shadow: 0 1px 4px rgba(0,0,0,0.1);
    }
    .card-header { display: flex; justify-content: space-between; margin-bottom: 8px; }
    .badge { padding: 2px 8px; border-radius: 10px; font-size: 0.75rem; background: #e0e0e0; }
    .badge.active { background: #dc3545; color: white; }
    .call-type { font-size: 0.8rem; color: #666; }
    .session-card p { margin: 4px 0; font-size: 0.85rem; color: #555; }
    .agent { font-weight: 600; color: #333 !important; }
  `]
})
export class DashboardComponent implements OnInit {
  sessions: ActiveSession[] = [];

  ngOnInit(): void {
    this.loadActiveSessions();
  }

  async loadActiveSessions(): Promise<void> {
    try {
      const response = await fetch(`${environment.apiUrl}/callsession/active`);
      if (response.ok) {
        this.sessions = await response.json();
      }
    } catch {
      // API not available yet
    }
  }

  formatDuration(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }
}
