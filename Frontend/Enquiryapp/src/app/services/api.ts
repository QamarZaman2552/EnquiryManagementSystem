import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class Api {
    private baseUrl = environment.baseUrl;

    constructor(private http: HttpClient) { }

    // ── SERVICES ──────────────────────────────────────────────
    // GET all services
    getServices(): Observable<any[]> {
        return this.http.get<any[]>(`${this.baseUrl}/service`);
    }
    // POST add new service
    addNewServices(data: any): Observable<any> {
        return this.http.post(`${this.baseUrl}/service`, data);
    }
    // DELETE service by id
    deleteService(id: number): Observable<any> {
        return this.http.delete(`${this.baseUrl}/service/${id}`);
    }
    // PUT update service by id
    updateService(id: number, data: any): Observable<any> {
        return this.http.put(`${this.baseUrl}/service/${id}`, data);
    }

    // ── ENQUIRIES ─────────────────────────────────────────────
    // GET all enquiries (with service name via LINQ JOIN)
    getEnquiresWithNames(): Observable<any[]> {
        return this.http.get<any[]>(`${this.baseUrl}/Enquiry`);
    }
    // POST submit new enquiry
    addNewEnquiry(data: any): Observable<any> {
        return this.http.post(`${this.baseUrl}/Enquiry`, data);
    }
    // DELETE enquiry by id
    deleteEnquiry(id: number): Observable<any> {
        return this.http.delete(`${this.baseUrl}/Enquiry/${id}`);
    }
    // PATCH update enquiry status
    updateStatus(id: number, status: string): Observable<any> {
        return this.http.patch(`${this.baseUrl}/Enquiry/status/${id}`, { status });
    }
}
