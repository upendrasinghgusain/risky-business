import { Routes } from '@angular/router';
import { GlossaryPage } from './glossary/glossary-page';

export const routes: Routes = [
  { path: '', component: GlossaryPage, title: 'Glossary · Risky Business' },
  { path: '**', redirectTo: '' },
];
