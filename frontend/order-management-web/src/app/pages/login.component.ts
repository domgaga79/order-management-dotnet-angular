import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../core/auth.service';

@Component({
  standalone: true,
  imports: [FormsModule],
  template: `
    <section class="auth-shell">
      <div class="auth-card">
        <div class="brand-mark">OM</div>
        <p class="eyebrow">Order Management</p>
        <h1>Entrar</h1>
        <p class="muted">Acesse o painel administrativo do sistema.</p>

        <label>
          E-mail
          <input
            [(ngModel)]="email"
            type="email"
            autocomplete="username"
            placeholder="admin@local.test"
            (keyup.enter)="go()"
          />
        </label>

        <label>
          Senha
          <input
            [(ngModel)]="password"
            type="password"
            autocomplete="current-password"
            placeholder="Sua senha"
            (keyup.enter)="go()"
          />
        </label>

        <button class="primary full" [disabled]="loading" (click)="go()">
          {{ loading ? 'Entrando...' : 'Entrar' }}
        </button>

        @if (error) {
          <div class="alert error">{{ error }}</div>
        }

        <div class="demo-box">
          <strong>Acesso local de desenvolvimento</strong>
          <span>admin&#64;local.test</span>
          <span>Admin123!</span>
        </div>
      </div>
    </section>
  `
})
export class LoginComponent {
  email = 'admin@local.test';
  password = 'Admin123!';
  error = '';
  loading = false;

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  go(): void {
    if (this.loading) return;

    this.error = '';
    this.loading = true;

    this.auth
      .login(this.email.trim(), this.password)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => this.router.navigateByUrl('/'),
        error: (error: HttpErrorResponse) => {
          if (error.status === 0) {
            this.error = 'A API está indisponível. Inicie o backend na porta 5294.';
          } else if (error.status === 401) {
            this.error = 'E-mail ou senha inválidos.';
          } else {
            this.error = error.error?.message ?? `Falha no login (HTTP ${error.status}).`;
          }
        }
      });
  }
}
