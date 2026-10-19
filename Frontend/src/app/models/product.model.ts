export interface CreateProductRequest {
  code: string;
  name: string;
  description: string;
  price: number;
  commercialLineName: string;
  categoryName: string;
  images: File[];
}

export interface UpdateProductRequest {
  name: string;
  description: string;
  price: number;
  commercialLineName: string;
  categoryName: string;
  images?: File[];
}

export interface SearchProductRequest {
  name?: string;
  commercialLineId?: string;
  categoryId?: string;
  page?: number;
  pageSize?: number;
}

export interface ProductSummary {
  id: string;
  code: string;
  name: string;
  description: string;
  price: number;
  commercialLineName: string;
  categoryName: string;
  imageUrls: string[];
  isActive: boolean;
}
