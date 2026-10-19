export interface CreateShippingTypeRequest {
  name: string;
  cost: number;
}

export interface UpdateShippingTypeRequest {
  name: string;
  cost: number;
}

export interface ShippingTypeResponse {
  id: string;
  name: string;
  cost: number;
  isDefault: boolean;
}
