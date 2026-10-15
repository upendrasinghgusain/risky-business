import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface GlossaryTerm {
  id: string;
  term: string;
  definition: string;
  createdAt: string;
}

export interface CreateGlossaryTerm {
  term: string;
  definition: string;
}

@Injectable({ providedIn: 'root' })
export class GlossaryApi {
  private readonly http = inject(HttpClient);

  list(): Observable<GlossaryTerm[]> {
    return this.http.get<GlossaryTerm[]>('/api/glossary');
  }

  create(request: CreateGlossaryTerm): Observable<GlossaryTerm> {
    return this.http.post<GlossaryTerm>('/api/glossary', request);
  }
}
