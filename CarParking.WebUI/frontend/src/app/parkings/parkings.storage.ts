import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, publishLast, refCount, switchMap } from 'rxjs/operators';
import { CompletionResult, CompletionResultType, Parking, StartedFree } from './models';
import { ParkingsFilter } from './parkings-filter/parking-filter';
import { ParkingsFilterStorage } from './parkings-filter/parkings-filter.storage';
import { ParkingsApi } from './parkings.api';

type Parkings$ = Observable<readonly Parking[]>;

@Injectable({ providedIn: 'root' })
export class ParkingsStorage {
    public readonly all: Observable<readonly Parking[]>;
    private readonly parkings$ = new BehaviorSubject<Parkings$>(of([]));

    constructor(
        private readonly parkingsApi: ParkingsApi,
        private readonly parkingsFilterStorage: ParkingsFilterStorage,
    ) {
        this.all = this.parkings$.pipe(
            switchMap(x => x),
            map(this.sortParkings),
        );
    }

    public loadStorage(filter$: Observable<ParkingsFilter>) {
        const loadedParkings$ = filter$
            .pipe(
                switchMap(filter => {
                    const httpParams = this.parkingsFilterStorage.toHttpParams(filter);

                    return this.parkingsApi.getAll(httpParams)
                        .pipe(
                            publishLast(),
                            refCount(),
                        );
                }),
            );

        this.parkings$.next(loadedParkings$);
    }

    public async create(): Promise<void> {
        await this.parkingsApi.create();
        this.reloadStorage();
    }

    public async completeParking(parking: StartedFree): Promise<CompletionResult> {
        const result = await this.parkingsApi.complete(parking);

        if (result.type === CompletionResultType.Success) {
            this.reloadStorage();
        }

        return result;
    }

    public async payParking(parking: Parking): Promise<void> {
        await this.parkingsApi.pay(parking);
        this.reloadStorage();
    }

    private sortParkings(parkings: readonly Parking[]): readonly Parking[] {
        return [...parkings].sort((a, b) => a.arrivalDate.valueOf() - b.arrivalDate.valueOf());
    }

    private reloadStorage() {
        const filter$ = this.parkingsFilterStorage.filter;
        this.loadStorage(filter$);
    }
}
