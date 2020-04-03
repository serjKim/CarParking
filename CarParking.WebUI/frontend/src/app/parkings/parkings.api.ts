import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { deserialize } from 'santee-dcts';
import { AppSettings } from '../app-settings';
import { CompletionResult, CompletionResultType } from './models/completion';
import { Parking, ParkingReadModel, ParkingsReadModel, StartedFree } from './models/parking';

const enum TransitionErrorType {
    FreeExpired = 'FreeExpired',
}

interface TransitionErrorResponse {
    readonly errorType: TransitionErrorType;
}

@Injectable()
export class ParkingsApi {
    constructor(
        private readonly httpClient: HttpClient,
        private readonly settings: AppSettings,
    ) { }

    public getAll(queryParams: HttpParams): Observable<readonly Parking[]> {
        return this.httpClient.get(`${this.settings.apiUrl}/parkings`, { params: queryParams }).pipe(
            map(obj => {
                return deserialize(obj, ParkingsReadModel).parkings;
            }),
        );
    }

    public create(): Promise<Parking> {
        return this.httpClient.post(`${this.settings.apiUrl}/parkings`, null)
            .pipe(map((obj: object) => deserialize(obj, ParkingReadModel).parking))
            .toPromise();
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
