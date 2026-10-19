export interface SearchAuditRequest {
  dateFrom: string;
  dateTo: string;
  entityName: string;
  entityId?: string;
}

export interface AuditLogResponse {
  id: string;
  username: string;
  time: string;
  entityName: string;
  entityId: string;
  description: string;
}
