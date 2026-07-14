import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { Service, Enquiry, ContactFormData } from '../models/interfaces';

@Injectable({ providedIn: 'root' })
export class Api {
    private baseUrl = environment.baseUrl;

    constructor(private http: HttpClient) { }

    // ── SERVICES ──────────────────────────────────────────────
    getServices(): Observable<Service[]> {
        return this.http.get<Service[]>(`${this.baseUrl}/service`);
    }

    addNewServices(data: Partial<Service>): Observable<Service> {
        return this.http.post<Service>(`${this.baseUrl}/service`, data);
    }

    deleteService(id: number): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/service/${id}`);
    }

    updateService(id: number, data: Partial<Service>): Observable<void> {
        return this.http.put<void>(`${this.baseUrl}/service/${id}`, data);
    }

    // ── ENQUIRIES ─────────────────────────────────────────────
    getEnquiresWithNames(): Observable<Enquiry[]> {
        return this.http.get<Enquiry[]>(`${this.baseUrl}/Enquiry`);
    }

    addNewEnquiry(data: Record<string, unknown>): Observable<Enquiry> {
        return this.http.post<Enquiry>(`${this.baseUrl}/Enquiry`, data);
    }

    deleteEnquiry(id: number): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/Enquiry/${id}`);
    }

    updateStatus(id: number, status: string): Observable<{ status: string }> {
        return this.http.patch<{ status: string }>(`${this.baseUrl}/Enquiry/status/${id}`, { status });
    }

    // ── CONTACT ───────────────────────────────────────────────
    sendContactMessage(data: ContactFormData): Observable<{ message: string }> {
        return this.http.post<{ message: string }>(`${this.baseUrl}/Contact`, data);
    }
}
