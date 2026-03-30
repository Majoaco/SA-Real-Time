import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { LiveCallComponent } from './features/live-call/live-call.component';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'live-call', component: LiveCallComponent },
  { path: '**', redirectTo: '' }
];
