import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppSettings } from '../app-settings';
import { deserializeParking, deserializeParkings, ParkingRaw } from './dtos';
import { CompletionResult, CompletionResultType, Parking, StartedFree } from './models';

const enum TransitionErrorType {
    FreeExpired = 'FreeExpired',
}

interface TransitionErrorResponse {
    readonly errorType: TransitionErrorType;
}

@Injectable({ providedIn: 'root' })
export class ParkingsApi {
    constructor(
        private readonly httpClient: HttpClient,
        private readonly settings: AppSettings,
    ) { }

    public getAll(queryParams: HttpParams): Observable<readonly Parking[]> {
        return this.httpClient.get<ParkingRaw[]>(`${this.settings.apiUrl}/parkings`, { params: queryParams }).pipe(
            map(obj => deserializeParkings(obj)),
        );
    }

    public create(): Promise<Parking> {
        return this.httpClient.post<ParkingRaw>(`${this.settings.apiUrl}/parkings`, null).pipe(
            map(obj => deserializeParking(obj)),
        ).toPromise();
    }

    public async complete(parking: StartedFree): Promise<CompletionResult> {
        try {
            const formData = new FormData();
            formData.append('status', 'Completed');

            await this.httpClient
                .patch(`${this.settings.apiUrl}/parkings/${parking.id}`, formData)
                .toPromise();

            return { type: CompletionResultType.Success };
        } catch (e) {
            if (e instanceof HttpErrorResponse) {
                if (e.status === 422) {
                    const error: TransitionErrorResponse = e.error;

                    if (error.errorType === TransitionErrorType.FreeExpired) {
                        return { type: CompletionResultType.FreeExpired };
                    }
                }
            }
            throw e;
        }
    }

    public pay(parking: Parking): Promise<unknown> {
        return this.httpClient
            .post(`${this.settings.apiUrl}/parkings/${parking.id}/payments`, null)
            .toPromise();
    }
}
