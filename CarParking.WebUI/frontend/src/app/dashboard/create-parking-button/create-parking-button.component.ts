import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ParkingsStorage } from 'src/app/parkings/parkings.storage';

@Component({
    selector: 'create-parking-button',
    templateUrl: './create-parking-button.component.html',
    styleUrls: ['./create-parking-button.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateParkingButtonComponent {
    constructor(private readonly parkingsStorage: ParkingsStorage) { }

    public onCreate() {
        this.parkingsStorage.create();
    }
}
