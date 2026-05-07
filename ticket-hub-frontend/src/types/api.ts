export interface ApiErrorResponse {
  errorCode: string;
  message: string;
  timestamp: string;
  traceId?: string | null;
}

export class ApiError extends Error {
  status: number;
  code: string;
  traceId?: string | null;

  constructor(message: string, status: number, code: string, traceId?: string | null) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
    this.traceId = traceId;
  }
}
