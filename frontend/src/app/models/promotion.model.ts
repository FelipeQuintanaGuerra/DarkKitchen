export interface CreatePromotionRequest {
  name: string;
  startDate: string;
  endDate: string;
  discount: number;
  commercialLineId: string;
  productIds?: string[];
}

export interface UpdatePromotionRequest {
  name: string;
  startDate: string;
  endDate: string;
  discount: number;
}

export interface SearchPromotionRequest {
  dateFrom?: string;
  dateTo?: string;
  commercialLineId?: string;
  productId?: string;
  page?: number;
  pageSize?: number;
}

export interface PromotionSummary {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  discount: number;
  commercialLineName: string;
  productNames: string[];
}
