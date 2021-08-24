import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { notNullOrFail } from '../../../../extensions';
import { CompletedFirst, CompletedFree, Parking, ParkingType } from '../../../models';

@Component({
    selector: 'complete-info',
    templateUrl: './completed-info.component.html',
    styleUrls: ['../item-info.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompletedInfoComponent {
    @Input()
    public set parking(prk: Parking) {
        this.currentParking = prk;
    }

    public get parking(): Parking {
        return notNullOrFail(this.currentParking);
    }

    private currentParking: Parking | null = null;

    public get info(): CompletedFirst | CompletedFree | null {
        return this.parking.type === ParkingType.CompletedFirst || this.parking.type === ParkingType.CompletedFree
            ? this.parking
            : null;
    }
}
