import { ChangeDetectionStrategy, Component, HostListener, Input } from '@angular/core';
import { errorUnhandledType, notNullOrFail } from '../../util';
import { Parking, ParkingType } from '../models';
import { CompletedInfo, StartedInfo } from './item-info';

enum InfoType {
    Started,
    Completed,
}

interface InfoByType {
    [InfoType.Started]?: StartedInfo;
    [InfoType.Completed]?: CompletedInfo;
}

@Component({
    selector: 'parkings-list-item',
    templateUrl: './parkings-list-item.component.html',
    styleUrls: ['./parkings-list-item.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsListItemComponent {
    public readonly intoType = InfoType;
    public readonly infosByType: InfoByType = {};
    public selectedInfo = InfoType.Started;
    public canSwitchInfo = false;
    private currentParking: Parking | null = null;

    @Input()
    public set parking(prk: Parking) {
        this.currentParking = prk;
        this.infosByType[InfoType.Started] = prk;

        switch (prk.type) {
            case ParkingType.StartedFree:
                this.selectedInfo = InfoType.Started;
                break;
            case ParkingType.CompletedFirst:
            case ParkingType.CompletedFree:
                this.selectedInfo = InfoType.Completed;
                this.canSwitchInfo = true;
                this.infosByType[InfoType.Completed] = prk;
                break;
            default:
                throw errorUnhandledType(prk);
        }
    }

    public get parking(): Parking {
        return notNullOrFail(this.currentParking);
    }

    @HostListener('click')
    public onSwitchInfo() {
        if (!this.canSwitchInfo) {
            return;
        }

        this.selectedInfo = this.selectedInfo === InfoType.Started
            ? InfoType.Completed
            : InfoType.Started;
    }
}
