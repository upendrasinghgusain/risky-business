import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

type ApiStatus = 'checking' | 'healthy' | 'unavailable';

@Component({
  imports: [RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly http = inject(HttpClient);

  protected readonly apiStatus = signal<ApiStatus>('checking');

  ngOnInit(): void {
    this.http.get('/health/ready', { responseType: 'text' }).subscribe({
      next: () => this.apiStatus.set('healthy'),
      error: () => this.apiStatus.set('unavailable'),
    });
  }
}
