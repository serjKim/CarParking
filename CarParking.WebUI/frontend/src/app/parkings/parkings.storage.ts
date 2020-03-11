import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, publishLast, refCount, switchMap } from 'rxjs/operators';
import { CompletionResult, CompletionResultType } from './models/completion';
import { Parking, StartedFree } from './models/parking';
import { ParkingsApi } from './parkings.api';

type Parkings$ = Observable<readonly Parking[]>;

@Injectable()
export class ParkingsStorage {
    private readonly parkings$ = new BehaviorSubject<Parkings$>(of([]));

    public get all(): Observable<readonly Parking[]> {
        return this.parkings$.pipe(
            switchMap(x => x),
            map(this.sortParkings),
        );
    }

    constructor(
        private readonly parkingsApi: ParkingsApi,
    ) { }

    public loadStorage() {
        const loadedParkings$ = this.parkingsApi.getAll()
            .pipe(
                publishLast(),
                refCount(),
            );

        this.parkings$.next(loadedParkings$);
    }

    public async create(): Promise<void> {
        const newParking = await this.parkingsApi.create();

        const newSource$ = this.parkings$.value.pipe(
            map(parkings => [...parkings, newParking]),
        );
        this.parkings$.next(newSource$);
    }

    public async completeParking(parking: StartedFree): Promise<CompletionResult> {
        const result = await this.parkingsApi.complete(parking);

        if (result.type === CompletionResultType.Success) {
            this.loadStorage();
        }

        return result;
    }

    public async payParking(parking: Parking): Promise<void> {
        await this.parkingsApi.pay(parking);
        this.loadStorage();
    }

    private sortParkings(parkings: readonly Parking[]): readonly Parking[] {
        return [...parkings].sort((a, b) => a.arrivalDate.valueOf() - b.arrivalDate.valueOf());
    }
}
