import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Parking } from '../models';
import { ParkingsStorage } from '../parkings.storage';

@Component({
    selector: 'parkings-list',
    templateUrl: './parkings-list.component.html',
    styleUrls: ['./parkings-list.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsListComponent {
    public readonly parkings$ = this.parkingsStorage.all;

    constructor(private readonly parkingsStorage: ParkingsStorage) { }

    public trackById(_: number, prk: Parking) {
        return prk.id;
    }
}
