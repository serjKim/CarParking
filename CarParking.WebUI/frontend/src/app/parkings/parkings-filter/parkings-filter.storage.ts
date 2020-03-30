import { Injectable } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { PARKING_TYPE_KEYS, ParkingType, ParkingTypeKey } from '../models/parking';
import { ParkingsFilter } from './parking-filter';

export interface ParkingsFilterQueryParams extends Params {
    readonly types?: string | null;
}

@Injectable()
export class ParkingsFilterStorage {
    private readonly filter$: Observable<ParkingsFilter>;

    constructor(activatedRoute: ActivatedRoute, private readonly router: Router) {
        this.filter$ = activatedRoute.queryParams
            .pipe(
                map(params => this.deserializeFilter(params)),
            );
    }

    public get filter(): Observable<ParkingsFilter> {
        return this.filter$;
    }

    public applyFilter(filter$: Observable<ParkingsFilter>) {
        filter$.subscribe(filter => {
            this.router.navigate([], {
                queryParams: this.serializeFilter(filter),
            });
        });
    }

    private serializeFilter(filter: ParkingsFilter): ParkingsFilterQueryParams {
        return {
            types: filter.parkingTypeKeys.size > 0
                ? this.serializeParkingTypeKeys(filter.parkingTypeKeys)
                : null,
        };
    }

    private deserializeFilter(params: Params): ParkingsFilter {
        const keys = this.deserializeParkingTypeKeys(params);
        return new ParkingsFilter(keys);
    }

    private deserializeParkingTypeKeys = (params: Params): ReadonlySet<ParkingTypeKey> => {
        const types: string | null | undefined = (params as ParkingsFilterQueryParams).types;
        const parsedParkingTypes = types?.split(',').filter(this.isParkingTypeKey) ?? [];

        return new Set(parsedParkingTypes);
    }

    private isParkingTypeKey = (raw: string): raw is ParkingTypeKey => {
        return PARKING_TYPE_KEYS.has(raw as keyof typeof ParkingType);
    }

    private serializeParkingTypeKeys(keys: ReadonlySet<ParkingTypeKey>): string {
        return Array.from(keys).join(',');
    }
}
