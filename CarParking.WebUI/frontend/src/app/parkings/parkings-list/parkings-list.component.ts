import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ParkingsApi } from '../parkings.api';

@Component({
    selector: 'parkings-list',
    templateUrl: './parkings-list.component.html',
    styleUrls: ['./parkings-list.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsListComponent {
    constructor(public parkingsApi: ParkingsApi) {
        // tslint:disable-next-line: no-console
        parkingsApi.getAll().subscribe(x => console.log(x));
    }
}
