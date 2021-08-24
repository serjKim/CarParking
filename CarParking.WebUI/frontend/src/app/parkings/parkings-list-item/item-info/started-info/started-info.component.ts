import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { notNullOrFail } from '../../../../extensions';
import { Parking, ParkingType, StartedFree } from '../../../models';

@Component({
    selector: 'started-info',
    templateUrl: './started-info.component.html',
    styleUrls: ['../item-info.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StartedInfoComponent {
    private currentParking: StartedFree | null = null;

    @Input()
    public set parking(prk: Parking) {
        if (prk.type !== ParkingType.StartedFree) {
            throw new Error('Expected StartedFree.');
        }
        this.currentParking = prk;
    }

    public get info(): StartedFree {
        return notNullOrFail(this.currentParking);
    }
}
