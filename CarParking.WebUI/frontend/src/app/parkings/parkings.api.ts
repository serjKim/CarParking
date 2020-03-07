import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Parking } from './parking.model';

@Injectable()
export class ParkingsApi {
    constructor(private readonly httpClient: HttpClient) { }

    public getAll(): Observable<Parking> {
        return this.httpClient.get('http://localhost:5000/parkings').pipe(
            map((x) => x as Parking),
        );
    }
}
