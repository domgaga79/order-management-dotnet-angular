import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { DataService } from '../core/data.service';
import { Category, CategoryRequest } from '../core/models';

@Component({
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page-heading">
      <div>
        <p class="eyebrow">Catálogo</p>
        <h1>Categorias</h1>
        <p class="muted">Organize os produtos em grupos de negócio.</p>
      </div>
    </div>

    @if (message) { <div class="alert success">{{ message }}</div> }
    @if (error) { <div class="alert error">{{ error }}</div> }

    <section class="card form-card">
      <div class="card-title">
        <h2>{{ editingId ? 'Editar categoria' : 'Nova categoria' }}</h2>
      </div>

      <div class="form-grid compact">
        <label class="span-2">
          Nome
          <input [(ngModel)]="form.name" placeholder="Ex.: Notebooks" />
        </label>
        <label class="checkbox-row">
          <input [(ngModel)]="form.active" type="checkbox" />
          Ativa
        </label>
      </div>

      <div class="actions">
        <button class="primary" (click)="save()">
          {{ editingId ? 'Salvar alterações' : 'Adicionar categoria' }}
        </button>
        @if (editingId) {
          <button class="secondary" (click)="cancelEdit()">Cancelar</button>
        }
      </div>
    </section>

    <section class="card">
      <div class="card-title">
        <h2>Categorias cadastradas</h2>
        <span class="badge">{{ items.length }}</span>
      </div>

      <div class="table-wrap">
        <table>
          <thead>
            <tr><th>Nome</th><th>Status</th><th class="right">Ações</th></tr>
          </thead>
          <tbody>
            @for (item of items; track item.id) {
              <tr>
                <td><strong>{{ item.name }}</strong></td>
                <td><span class="status" [class.off]="!item.active">{{ item.active ? 'Ativa' : 'Inativa' }}</span></td>
                <td class="right table-actions">
                  <button class="ghost" (click)="edit(item)">Editar</button>
                  <button class="danger ghost" (click)="remove(item)">Excluir</button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="3" class="empty">Nenhuma categoria cadastrada.</td></tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class CategoriesComponent implements OnInit {
  items: Category[] = [];
  editingId: string | null = null;
  message = '';
  error = '';
  form: CategoryRequest = this.emptyForm();

  constructor(private readonly data: DataService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.data.categories().subscribe({
      next: items => (this.items = items),
      error: error => this.handleError(error)
    });
  }

  save(): void {
    this.clearFeedback();
    const request: CategoryRequest = { name: this.form.name.trim(), active: this.form.active };
    if (request.name.length < 2) {
      this.error = 'Informe um nome com pelo menos 2 caracteres.';
      return;
    }

    const operation = this.editingId
      ? this.data.updateCategory(this.editingId, request)
      : this.data.createCategory(request);

    operation.subscribe({
      next: () => {
        this.message = this.editingId ? 'Categoria atualizada.' : 'Categoria criada.';
        this.cancelEdit();
        this.load();
      },
      error: error => this.handleError(error)
    });
  }

  edit(item: Category): void {
    this.editingId = item.id;
    this.form = { name: item.name, active: item.active };
    this.clearFeedback();
  }

  cancelEdit(): void {
    this.editingId = null;
    this.form = this.emptyForm();
  }

  remove(item: Category): void {
    if (!confirm(`Excluir a categoria “${item.name}”?`)) return;

    this.clearFeedback();
    this.data.deleteCategory(item.id).subscribe({
      next: () => {
        this.message = 'Categoria excluída.';
        this.load();
      },
      error: error => this.handleError(error)
    });
  }

  private emptyForm(): CategoryRequest {
    return { name: '', active: true };
  }

  private clearFeedback(): void {
    this.message = '';
    this.error = '';
  }

  private handleError(error: HttpErrorResponse): void {
    this.error = error.error?.message ?? `Erro ao processar a categoria (HTTP ${error.status}).`;
  }
}
