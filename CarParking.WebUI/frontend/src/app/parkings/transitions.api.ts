import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { deserialize } from 'santee-dcts';
import { AppSettings } from '../app-settings';
import { Transition, TransitionReadModel } from './models/transition';

@Injectable()
export class TransitionsApi {
    constructor(
        private readonly httpClient: HttpClient,
        private readonly settings: AppSettings,
    ) { }

    public getTransitions(): Observable<Transition[]> {
        return this.httpClient.get(`${this.settings.apiUrl}/transitions`).pipe(
            map(obj => {
                return deserialize(obj, TransitionReadModel).transitions;
            }),
        );
    }
}
