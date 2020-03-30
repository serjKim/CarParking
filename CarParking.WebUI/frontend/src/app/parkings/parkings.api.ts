import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { deserialize } from 'santee-dcts';
import { AppSettings } from '../app-settings';
import { CompletionResult, CompletionResultType } from './models/completion';
import { Parking, ParkingReadModel, ParkingsReadModel, ParkingStatus, ParkingType, ParkingTypeKey, StartedFree, Tariff } from './models/parking';
import { ParkingsFilter } from './parkings-filter/parking-filter';

const enum TransitionErrorType {
    FreeExpired = 'FreeExpired',
}

interface TransitionErrorResponse {
    readonly errorType: TransitionErrorType;
}

type ByTypeFilterItems = {
    readonly [key in ParkingTypeKey]: ByTypeFilterItem
};

export interface ByTypeFilterItem {
    readonly tariff: Tariff;
    readonly status: ParkingStatus;
}

export class ParkingsApiFilter {
    private readonly byTypeFilterItems: ByTypeFilterItems = {
        [ParkingType.StartedFree]: { tariff: Tariff.Free, status: ParkingStatus.Started },
        [ParkingType.CompletedFree]: { tariff: Tariff.Free, status: ParkingStatus.Completed },
        [ParkingType.CompletedFirst]: { tariff: Tariff.First, status: ParkingStatus.Completed },
    };

    private readonly separator = '|';
    private readonly typesQueryKey = 'types';

    public toQueryParams(filter: ParkingsFilter): HttpParams {
        const params = new HttpParams();

        const byType = this.mapFilterItem(filter.parkingTypeKeys);

        if (byType.length > 0) {
            const types = byType.map(item => `${item.tariff}${this.separator}${item.status}`).join(',');
            return params.set(this.typesQueryKey, types);
        }

        return params;
    }

    private mapFilterItem(parkingTypeKeys: ReadonlySet<ParkingTypeKey>): readonly ByTypeFilterItem[] {
        return Array
            .from(parkingTypeKeys)
            .map(key => this.byTypeFilterItems[key]);
    }
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
