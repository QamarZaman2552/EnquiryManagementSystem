export interface Service {
  serviceId: number;
  serviceName: string;
  createdate: string;
  isActive: boolean;
  rate: number;
}

export interface Enquiry {
  id: number;
  fullName: string;
  email: string;
  mobile: string;
  subject: string;
  message: string;
  serviceId: number | null;
  serviceName: string;
  status: string;
  createdate: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: string;
}

export interface ContactFormData {
  fullName: string;
  email: string;
  phone: string;
  subject: string;
  message: string;
}

export interface EnquiryFormData {
  fullName: string;
  email: string;
  mobile: string;
  subject: string;
  message: string;
  serviceId: number | null;
}
