import { Component, OnInit } from '@angular/core';

interface QuickLink {
  label: string;
  description: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  userName = '';
  userRole = '';

  private roleMap: { [key: number]: string } = {
    0: 'Client',
    1: 'Dispatcher',
    2: 'Admin'
  };

  private linksByRole: { [role: string]: QuickLink[] } = {
    Admin: [
      { label: 'Productos', description: 'Administrá el catálogo de productos', icon: 'restaurant_menu', route: '/products' },
      { label: 'Promociones', description: 'Gestioná descuentos y promociones', icon: 'local_offer', route: '/promotions' },
      { label: 'Usuarios', description: 'Administrá los usuarios del sistema', icon: 'people', route: '/users' },
      { label: 'Tipos de envío', description: 'Configurá los tipos de envío', icon: 'local_shipping', route: '/shipping-types' },
      { label: 'Importadores', description: 'Importá productos desde archivos', icon: 'upload_file', route: '/importers' },
      { label: 'Detalle de pedido', description: 'Consultá el detalle de un pedido', icon: 'receipt_long', route: '/orders/detail' },
      { label: 'Top productos', description: 'Top 5 productos más vendidos', icon: 'bar_chart', route: '/reports/top-products' },
      { label: 'Ventas agrupadas', description: 'Ventas agrupadas por mes', icon: 'calendar_month', route: '/reports/sales' },
      { label: 'Auditoría', description: 'Revisá el log de auditoría', icon: 'history', route: '/audits' },
    ],
    Dispatcher: [
      { label: 'Pedidos filtrados', description: 'Buscá y gestioná pedidos', icon: 'receipt_long', route: '/orders/filtered' },
      { label: 'Detalle de pedido', description: 'Consultá el detalle de un pedido', icon: 'search', route: '/orders/detail' },
      { label: 'Top productos', description: 'Top 5 productos más vendidos', icon: 'bar_chart', route: '/reports/top-products' },
      { label: 'Ventas agrupadas', description: 'Ventas agrupadas por mes', icon: 'calendar_month', route: '/reports/sales' },
    ],
    Client: [
      { label: 'Productos', description: 'Explorá nuestro menú', icon: 'restaurant_menu', route: '/products' },
      { label: 'Crear pedido', description: 'Hacé un nuevo pedido', icon: 'add_shopping_cart', route: '/orders/create' },
      { label: 'Mis pedidos', description: 'Revisá el estado de tus pedidos', icon: 'receipt_long', route: '/orders/my-orders' },
    ]
  };

  quickLinks: QuickLink[] = [];

  ngOnInit(): void {
    const userJson = localStorage.getItem('user');
    if (!userJson) return;

    const user = JSON.parse(userJson);
    this.userName = user.name || '';

    const role = user.role;
    this.userRole = typeof role === 'number'
      ? this.roleMap[role] || ''
      : role || '';

    this.quickLinks = this.linksByRole[this.userRole] || [];
  }
}
