import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { DataService } from '../core/data.service';
import { Category, Product, ProductRequest } from '../core/models';

@Component({
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page-heading">
      <div>
        <p class="eyebrow">Catálogo e estoque</p>
        <h1>Produtos</h1>
        <p class="muted">Cadastre preços, estoque e categoria dos itens vendidos.</p>
      </div>
    </div>

    @if (message) { <div class="alert success">{{ message }}</div> }
    @if (error) { <div class="alert error">{{ error }}</div> }

    <section class="card form-card">
      <div class="card-title"><h2>{{ editingId ? 'Editar produto' : 'Novo produto' }}</h2></div>
      <div class="form-grid">
        <label>
          Nome
          <input [(ngModel)]="form.name" placeholder="Ex.: Notebook Dell" />
        </label>
        <label>
          Categoria
          <select [(ngModel)]="form.categoryId">
            <option [ngValue]="null">Sem categoria</option>
            @for (category of categories; track category.id) {
              <option [ngValue]="category.id">{{ category.name }}</option>
            }
          </select>
        </label>
        <label>
          Preço
          <input [(ngModel)]="form.price" type="number" min="0.01" step="0.01" />
        </label>
        <label>
          Estoque
          <input [(ngModel)]="form.stock" type="number" min="0" step="1" />
        </label>
        <label class="span-2">
          Descrição
          <textarea [(ngModel)]="form.description" rows="3" placeholder="Descrição opcional"></textarea>
        </label>
        <label class="checkbox-row">
          <input [(ngModel)]="form.active" type="checkbox" />
          Produto ativo
        </label>
      </div>
      <div class="actions">
        <button class="primary" (click)="save()">{{ editingId ? 'Salvar alterações' : 'Adicionar produto' }}</button>
        @if (editingId) { <button class="secondary" (click)="cancelEdit()">Cancelar</button> }
      </div>
    </section>

    <section class="card">
      <div class="card-title">
        <h2>Produtos cadastrados</h2>
        <span class="badge">{{ items.length }}</span>
      </div>
      <div class="table-wrap">
        <table>
          <thead>
            <tr><th>Produto</th><th>Categoria</th><th>Preço</th><th>Estoque</th><th>Status</th><th class="right">Ações</th></tr>
          </thead>
          <tbody>
            @for (product of items; track product.id) {
              <tr>
                <td><strong>{{ product.name }}</strong><small>{{ product.description || 'Sem descrição' }}</small></td>
                <td>{{ product.categoryName || '—' }}</td>
                <td>{{ money(product.price) }}</td>
                <td><span [class.low-stock]="product.stock <= 3">{{ product.stock }}</span></td>
                <td><span class="status" [class.off]="!product.active">{{ product.active ? 'Ativo' : 'Inativo' }}</span></td>
                <td class="right table-actions">
                  <button class="ghost" (click)="edit(product)">Editar</button>
                  <button class="danger ghost" (click)="remove(product)">Excluir</button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="6" class="empty">Nenhum produto cadastrado.</td></tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class ProductsComponent implements OnInit {
  items: Product[] = [];
  categories: Category[] = [];
  editingId: string | null = null;
  message = '';
  error = '';
  form: ProductRequest = this.emptyForm();

  constructor(private readonly data: DataService) {}

  ngOnInit(): void {
    this.load();
    this.data.categories().subscribe({ next: items => (this.categories = items.filter(x => x.active)) });
  }

  load(): void {
    this.data.products().subscribe({
      next: items => (this.items = items),
      error: error => this.handleError(error)
    });
  }

  save(): void {
    this.clearFeedback();
    const request: ProductRequest = {
      ...this.form,
      name: this.form.name.trim(),
      description: this.form.description?.trim() || null,
      price: Number(this.form.price),
      stock: Number(this.form.stock),
      categoryId: this.form.categoryId || null
    };

    if (request.name.length < 2 || request.price <= 0 || request.stock < 0) {
      this.error = 'Revise nome, preço e estoque do produto.';
      return;
    }

    const operation = this.editingId
      ? this.data.updateProduct(this.editingId, request)
      : this.data.createProduct(request);

    operation.subscribe({
      next: () => {
        this.message = this.editingId ? 'Produto atualizado.' : 'Produto criado.';
        this.cancelEdit();
        this.load();
      },
      error: error => this.handleError(error)
    });
  }

  edit(product: Product): void {
    this.editingId = product.id;
    this.form = {
      name: product.name,
      description: product.description ?? null,
      price: product.price,
      stock: product.stock,
      active: product.active,
      categoryId: product.categoryId ?? null
    };
    this.clearFeedback();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit(): void {
    this.editingId = null;
    this.form = this.emptyForm();
  }

  remove(product: Product): void {
    if (!confirm(`Excluir o produto “${product.name}”?`)) return;

    this.clearFeedback();
    this.data.deleteProduct(product.id).subscribe({
      next: () => {
        this.message = 'Produto excluído.';
        this.load();
      },
      error: error => this.handleError(error)
    });
  }

  money(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  private emptyForm(): ProductRequest {
    return { name: '', description: null, price: 1, stock: 0, active: true, categoryId: null };
  }

  private clearFeedback(): void {
    this.message = '';
    this.error = '';
  }

  private handleError(error: HttpErrorResponse): void {
    this.error = error.error?.message ?? `Erro ao processar o produto (HTTP ${error.status}).`;
  }
}
