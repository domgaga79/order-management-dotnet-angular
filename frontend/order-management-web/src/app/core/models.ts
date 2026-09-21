export interface AuthResponse {
  token: string;
  expiresAt: string;
  userId: string;
  name: string;
  email: string;
  role: 'Admin' | 'Operator' | string;
}

export interface Category {
  id: string;
  name: string;
  active: boolean;
  createdAt: string;
}

export interface CategoryRequest {
  name: string;
  active: boolean;
}

export interface Product {
  id: string;
  name: string;
  description?: string | null;
  price: number;
  stock: number;
  active: boolean;
  categoryId?: string | null;
  categoryName?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface ProductRequest {
  name: string;
  description?: string | null;
  price: number;
  stock: number;
  active: boolean;
  categoryId?: string | null;
}

export interface Customer {
  id: string;
  name: string;
  email?: string | null;
  phone?: string | null;
  active: boolean;
  createdAt: string;
}

export interface CustomerRequest {
  name: string;
  email?: string | null;
  phone?: string | null;
  active: boolean;
}

export interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface Order {
  id: string;
  customerId: string;
  customerName: string;
  status: 'Pending' | 'Completed' | 'Cancelled' | string;
  total: number;
  createdAt: string;
  items: OrderItem[];
}

export interface CreateOrderRequest {
  customerId: string;
  items: Array<{ productId: string; quantity: number }>;
}
