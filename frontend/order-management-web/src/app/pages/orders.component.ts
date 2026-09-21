import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { DataService } from '../core/data.service';
import { AuthService } from '../core/auth.service';
import { Customer, Order, Product } from '../core/models';

interface DraftItem {
  productId: string;
  quantity: number;
}

@Component({
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page-heading">
      <div>
        <p class="eyebrow">Operação</p>
        <h1>Pedidos</h1>
        <p class="muted">Crie pedidos, acompanhe status e observe a baixa transacional do estoque.</p>
      </div>
    </div>

    @if (message) { <div class="alert success">{{ message }}</div> }
    @if (error) { <div class="alert error">{{ error }}</div> }

    <section class="card form-card">
      <div class="card-title"><h2>Novo pedido</h2></div>

      <div class="form-grid compact">
        <label class="span-2">
          Cliente
          <select [(ngModel)]="customerId">
            <option value="">Selecione um cliente</option>
            @for (customer of activeCustomers; track customer.id) {
              <option [value]="customer.id">{{ customer.name }}</option>
            }
          </select>
        </label>
      </div>

      <div class="order-lines">
        @for (line of draftItems; track line; let i = $index) {
          <div class="order-line">
            <select [(ngModel)]="line.productId">
              <option value="">Produto</option>
              @for (product of activeProducts; track product.id) {
                <option [value]="product.id">{{ product.name }} · estoque {{ product.stock }}</option>
              }
            </select>
            <input [(ngModel)]="line.quantity" type="number" min="1" step="1" />
            <button class="danger ghost" [disabled]="draftItems.length === 1" (click)="removeLine(i)">Remover</button>
          </div>
        }
      </div>

      <div class="actions">
        <button class="secondary" (click)="addLine()">+ Adicionar item</button>
        <button class="primary" (click)="save()">Criar pedido</button>
      </div>
    </section>

    <section class="card">
      <div class="card-title"><h2>Pedidos recentes</h2><span class="badge">{{ items.length }}</span></div>

      <div class="order-list">
        @for (order of items; track order.id) {
          <article class="order-card">
            <div class="order-head">
              <div>
                <small>{{ date(order.createdAt) }}</small>
                <h3>{{ order.customerName }}</h3>
              </div>
              <div class="order-summary">
                <span class="status" [class.done]="order.status === 'Completed'" [class.cancelled]="order.status === 'Cancelled'">{{ statusLabel(order.status) }}</span>
                <strong>{{ money(order.total) }}</strong>
              </div>
            </div>

            <div class="order-items">
              @for (item of order.items; track item.productId) {
                <div>
                  <span>{{ item.quantity }}× {{ item.productName }}</span>
                  <span>{{ money(item.subtotal) }}</span>
                </div>
              }
            </div>

            @if (order.status === 'Pending') {
              <div class="actions right-actions">
                <button class="success-btn" (click)="complete(order)">Concluir</button>
                @if (auth.isAdmin()) {
                  <button class="danger ghost" (click)="cancel(order)">Cancelar e devolver estoque</button>
                }
              </div>
            }
          </article>
        } @empty {
          <div class="empty">Nenhum pedido criado.</div>
        }
      </div>
    </section>
  `
})
export class OrdersComponent implements OnInit {
  items: Order[] = [];
  customers: Customer[] = [];
  products: Product[] = [];
  customerId = '';
  draftItems: DraftItem[] = [{ productId: '', quantity: 1 }];
  message = '';
  error = '';

  constructor(
    private readonly data: DataService,
    public readonly auth: AuthService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  get activeCustomers(): Customer[] {
    return this.customers.filter(x => x.active);
  }

  get activeProducts(): Product[] {
    return this.products.filter(x => x.active);
  }

  load(): void {
    this.data.orders().subscribe({
      next: items => (this.items = items),
      error: error => this.handleError(error)
    });
    this.data.customers().subscribe({ next: items => (this.customers = items) });
    this.data.products().subscribe({ next: items => (this.products = items) });
  }

  addLine(): void {
    this.draftItems.push({ productId: '', quantity: 1 });
  }

  removeLine(index: number): void {
    if (this.draftItems.length > 1) this.draftItems.splice(index, 1);
  }

  save(): void {
    this.clearFeedback();
    const items = this.draftItems
      .filter(x => x.productId && Number(x.quantity) > 0)
      .map(x => ({ productId: x.productId, quantity: Number(x.quantity) }));

    if (!this.customerId || items.length === 0) {
      this.error = 'Selecione o cliente e ao menos um item válido.';
      return;
    }

    this.data.createOrder({ customerId: this.customerId, items }).subscribe({
      next: () => {
        this.message = 'Pedido criado e estoque atualizado.';
        this.customerId = '';
        this.draftItems = [{ productId: '', quantity: 1 }];
        this.load();
      },
      error: error => this.handleError(error)
    });
  }

  complete(order: Order): void {
    this.clearFeedback();
    this.data.completeOrder(order.id).subscribe({
      next: () => {
        this.message = 'Pedido concluído.';
        this.load();
      },
      error: error => this.handleError(error)
    });
  }

  cancel(order: Order): void {
    if (!confirm('Cancelar este pedido e devolver os itens ao estoque?')) return;

    this.clearFeedback();
    this.data.cancelOrder(order.id).subscribe({
      next: () => {
        this.message = 'Pedido cancelado e estoque restaurado.';
        this.load();
      },
      error: error => this.handleError(error)
    });
  }

  statusLabel(status: string): string {
    return ({ Pending: 'Pendente', Completed: 'Concluído', Cancelled: 'Cancelado' } as Record<string, string>)[status] ?? status;
  }

  money(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  date(value: string): string {
    return new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value));
  }

  private clearFeedback(): void {
    this.message = '';
    this.error = '';
  }

  private handleError(error: HttpErrorResponse): void {
    this.error = error.error?.message ?? `Erro ao processar o pedido (HTTP ${error.status}).`;
  }
}
