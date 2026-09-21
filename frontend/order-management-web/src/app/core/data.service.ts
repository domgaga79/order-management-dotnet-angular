import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API } from './api';
import {
  Category,
  CategoryRequest,
  CreateOrderRequest,
  Customer,
  CustomerRequest,
  Order,
  Product,
  ProductRequest
} from './models';

@Injectable({ providedIn: 'root' })
export class DataService {
  constructor(private readonly http: HttpClient) {}

  categories() {
    return this.http.get<Category[]>(`${API}/categories`);
  }

  createCategory(request: CategoryRequest) {
    return this.http.post<Category>(`${API}/categories`, request);
  }

  updateCategory(id: string, request: CategoryRequest) {
    return this.http.put<Category>(`${API}/categories/${id}`, request);
  }

  deleteCategory(id: string) {
    return this.http.delete<void>(`${API}/categories/${id}`);
  }

  products() {
    return this.http.get<Product[]>(`${API}/products`);
  }

  createProduct(request: ProductRequest) {
    return this.http.post<Product>(`${API}/products`, request);
  }

  updateProduct(id: string, request: ProductRequest) {
    return this.http.put<Product>(`${API}/products/${id}`, request);
  }

  deleteProduct(id: string) {
    return this.http.delete<void>(`${API}/products/${id}`);
  }

  customers() {
    return this.http.get<Customer[]>(`${API}/customers`);
  }

  createCustomer(request: CustomerRequest) {
    return this.http.post<Customer>(`${API}/customers`, request);
  }

  updateCustomer(id: string, request: CustomerRequest) {
    return this.http.put<Customer>(`${API}/customers/${id}`, request);
  }

  deleteCustomer(id: string) {
    return this.http.delete<void>(`${API}/customers/${id}`);
  }

  orders() {
    return this.http.get<Order[]>(`${API}/orders`);
  }

  createOrder(request: CreateOrderRequest) {
    return this.http.post<Order>(`${API}/orders`, request);
  }

  completeOrder(id: string) {
    return this.http.post<void>(`${API}/orders/${id}/complete`, {});
  }

  cancelOrder(id: string) {
    return this.http.post<void>(`${API}/orders/${id}/cancel`, {});
  }
}
