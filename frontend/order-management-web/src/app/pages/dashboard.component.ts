import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DataService } from '../core/data.service';
import { Order } from '../core/models';

@Component({
  standalone: true,
  template: `
    <div class="page-heading">
      <div>
        <p class="eyebrow">Visão geral</p>
        <h1>Dashboard</h1>
        <p class="muted">Resumo operacional do ambiente de desenvolvimento.</p>
      </div>
    </div>

    <section class="stats-grid">
      <article class="stat-card"><span>Produtos</span><strong>{{ products }}</strong><small>itens cadastrados</small></article>
      <article class="stat-card"><span>Clientes</span><strong>{{ customers }}</strong><small>clientes cadastrados</small></article>
      <article class="stat-card"><span>Pedidos pendentes</span><strong>{{ pending }}</strong><small>aguardando conclusão</small></article>
      <article class="stat-card"><span>Faturamento concluído</span><strong class="money-stat">{{ money(completedRevenue) }}</strong><small>pedidos concluídos</small></article>
    </section>

    <section class="grid two">
      <article class="card">
        <div class="card-title"><h2>Arquitetura</h2></div>
        <div class="architecture-flow">
          <span>Angular 17</span><b>→</b><span>ASP.NET Core 8</span><b>→</b><span>EF Core</span><b>→</b><span>PostgreSQL 16</span>
        </div>
        <p class="muted">JWT, roles, REST, migrations, Docker e regras transacionais de pedido/estoque.</p>
      </article>

      <article class="card">
        <div class="card-title"><h2>Últimos pedidos</h2></div>
        <div class="mini-list">
          @for (order of recentOrders; track order.id) {
            <div><span>{{ order.customerName }}</span><strong>{{ money(order.total) }}</strong></div>
          } @empty {
            <p class="muted">Ainda não há pedidos.</p>
          }
        </div>
      </article>
    </section>
  `
})
export class DashboardComponent implements OnInit {
  products = 0;
  customers = 0;
  pending = 0;
  completedRevenue = 0;
  recentOrders: Order[] = [];

  constructor(private readonly data: DataService) {}

  ngOnInit(): void {
    forkJoin({
      products: this.data.products(),
      customers: this.data.customers(),
      orders: this.data.orders()
    }).subscribe(({ products, customers, orders }) => {
      this.products = products.length;
      this.customers = customers.length;
      this.pending = orders.filter(x => x.status === 'Pending').length;
      this.completedRevenue = orders
        .filter(x => x.status === 'Completed')
        .reduce((sum, order) => sum + order.total, 0);
      this.recentOrders = orders.slice(0, 5);
    });
  }

  money(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }
}
