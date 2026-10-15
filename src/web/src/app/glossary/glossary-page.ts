import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { GlossaryApi, GlossaryTerm } from './glossary-api';

@Component({
  selector: 'app-glossary-page',
  imports: [ReactiveFormsModule],
  templateUrl: './glossary-page.html',
  styleUrl: './glossary-page.css',
})
export class GlossaryPage implements OnInit {
  private readonly api = inject(GlossaryApi);

  protected readonly terms = signal<GlossaryTerm[]>([]);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saveError = signal<string | null>(null);
  protected readonly saving = signal(false);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    term: ['', [Validators.required, Validators.maxLength(100)]],
    definition: ['', [Validators.required, Validators.maxLength(2000)]],
  });

  ngOnInit(): void {
    this.load();
  }

  protected add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);

    this.api.create(this.form.getRawValue()).subscribe({
      next: () => {
        this.form.reset();
        this.saving.set(false);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.saveError.set(
          error.status === 409
            ? 'That term is already in the glossary.'
            : 'Could not save the term.',
        );
      },
    });
  }

  private load(): void {
    this.api.list().subscribe({
      next: (terms) => {
        this.terms.set(terms);
        this.loadError.set(null);
      },
      error: () => this.loadError.set('Could not load the glossary. Is the API running?'),
    });
  }
}
