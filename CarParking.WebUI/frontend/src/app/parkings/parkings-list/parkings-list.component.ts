import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { ParkingsStorage } from '../parkings.storage';

@Component({
    selector: 'parkings-list',
    templateUrl: './parkings-list.component.html',
    styleUrls: ['./parkings-list.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsListComponent implements OnInit {
    public readonly parkings$ = this.parkingsStorage.all;

    constructor(private readonly parkingsStorage: ParkingsStorage) { }

    public ngOnInit() {
        this.parkingsStorage.loadStorage();
    }
}
