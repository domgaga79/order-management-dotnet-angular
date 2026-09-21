import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { DataService } from '../core/data.service';
import { Customer, CustomerRequest } from '../core/models';

@Component({
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page-heading">
      <div>
        <p class="eyebrow">Relacionamento</p>
        <h1>Clientes</h1>
        <p class="muted">Mantenha os clientes disponíveis para criação de pedidos.</p>
      </div>
    </div>

    @if (message) { <div class="alert success">{{ message }}</div> }
    @if (error) { <div class="alert error">{{ error }}</div> }

    <section class="card form-card">
      <div class="card-title"><h2>{{ editingId ? 'Editar cliente' : 'Novo cliente' }}</h2></div>
      <div class="form-grid">
        <label>
          Nome
          <input [(ngModel)]="form.name" placeholder="Nome completo" />
        </label>
        <label>
          E-mail
          <input [(ngModel)]="form.email" type="email" placeholder="cliente@email.com" />
        </label>
        <label>
          Telefone
          <input [(ngModel)]="form.phone" placeholder="(00) 00000-0000" />
        </label>
        <label class="checkbox-row">
          <input [(ngModel)]="form.active" type="checkbox" />
          Cliente ativo
        </label>
      </div>
      <div class="actions">
        <button class="primary" (click)="save()">{{ editingId ? 'Salvar alterações' : 'Adicionar cliente' }}</button>
        @if (editingId) { <button class="secondary" (click)="cancelEdit()">Cancelar</button> }
      </div>
    </section>

    <section class="card">
      <div class="card-title"><h2>Clientes cadastrados</h2><span class="badge">{{ items.length }}</span></div>
      <div class="table-wrap">
        <table>
          <thead><tr><th>Nome</th><th>E-mail</th><th>Telefone</th><th>Status</th><th class="right">Ações</th></tr></thead>
          <tbody>
            @for (customer of items; track customer.id) {
              <tr>
                <td><strong>{{ customer.name }}</strong></td>
                <td>{{ customer.email || '—' }}</td>
                <td>{{ customer.phone || '—' }}</td>
                <td><span class="status" [class.off]="!customer.active">{{ customer.active ? 'Ativo' : 'Inativo' }}</span></td>
                <td class="right table-actions">
                  <button class="ghost" (click)="edit(customer)">Editar</button>
                  <button class="danger ghost" (click)="remove(customer)">Excluir</button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="5" class="empty">Nenhum cliente cadastrado.</td></tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class CustomersComponent implements OnInit {
  items: Customer[] = [];
  editingId: string | null = null;
  message = '';
  error = '';
  form: CustomerRequest = this.emptyForm();

  constructor(private readonly data: DataService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.data.customers().subscribe({
      next: items => (this.items = items),
      error: error => this.handleError(error)
    });
  }

  save(): void {
    this.clearFeedback();
    const request: CustomerRequest = {
      name: this.form.name.trim(),
      email: this.form.email?.trim() || null,
      phone: this.form.phone?.trim() || null,
      active: this.form.active
    };

    if (request.name.length < 2) {
      this.error = 'Informe o nome do cliente.';
      return;
    }

    const operation = this.editingId
      ? this.data.updateCustomer(this.editingId, request)
      : this.data.createCustomer(request);

    operation.subscribe({
      next: () => {
        this.message = this.editingId ? 'Cliente atualizado.' : 'Cliente criado.';
        this.cancelEdit();
        this.load();
      },
      error: error => this.handleError(error)
    });
  }

  edit(customer: Customer): void {
    this.editingId = customer.id;
    this.form = {
      name: customer.name,
      email: customer.email ?? null,
      phone: customer.phone ?? null,
      active: customer.active
    };
    this.clearFeedback();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit(): void {
    this.editingId = null;
    this.form = this.emptyForm();
  }

  remove(customer: Customer): void {
    if (!confirm(`Excluir o cliente “${customer.name}”?`)) return;

    this.clearFeedback();
    this.data.deleteCustomer(customer.id).subscribe({
      next: () => {
        this.message = 'Cliente excluído.';
        this.load();
      },
      error: error => this.handleError(error)
    });
  }

  private emptyForm(): CustomerRequest {
    return { name: '', email: '', phone: '', active: true };
  }

  private clearFeedback(): void {
    this.message = '';
    this.error = '';
  }

  private handleError(error: HttpErrorResponse): void {
    this.error = error.error?.message ?? `Erro ao processar o cliente (HTTP ${error.status}).`;
  }
}
