import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    @if (auth.isLoggedIn()) {
      <header class="topbar">
        <a routerLink="/" class="brand">
          <span class="brand-icon">OM</span>
          <span><strong>Order Management</strong><small>.NET + Angular</small></span>
        </a>

        <nav>
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Dashboard</a>
          <a routerLink="/categories" routerLinkActive="active">Categorias</a>
          <a routerLink="/products" routerLinkActive="active">Produtos</a>
          <a routerLink="/customers" routerLinkActive="active">Clientes</a>
          <a routerLink="/orders" routerLinkActive="active">Pedidos</a>
        </nav>

        <div class="user-box">
          <div><strong>{{ auth.user()?.name }}</strong><small>{{ auth.user()?.role }}</small></div>
          <button class="ghost" (click)="logout()">Sair</button>
        </div>
      </header>
    }

    <main [class.public-main]="!auth.isLoggedIn()">
      <router-outlet />
    </main>
  `
})
export class AppComponent {
  constructor(
    public readonly auth: AuthService,
    private readonly router: Router
  ) {}

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
