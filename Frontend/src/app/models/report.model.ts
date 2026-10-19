export interface TopProductDto {
  code: string;
  name: string;
  quantity: number;
  images: string[];
}

export interface CustomerSalesDto {
  customerId: string;
  customerName: string;
  total: number;
}

export interface MonthlySalesDto {
  period: string;
  customers: CustomerSalesDto[];
}
