import { ChangeDetectionStrategy, Component, HostListener, Input, OnInit } from '@angular/core';
import { assertUnhandledType } from '../../util';
import { Parking, ParkingType } from '../models/parking';

enum InfoType {
    Started,
    Completed,
}

@Component({
    selector: 'parkings-list-item',
    templateUrl: './parkings-list-item.component.html',
    styleUrls: ['./parkings-list-item.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsListItemComponent implements OnInit {
    public readonly intoType = InfoType;
    public selectedInfo = InfoType.Started;
    public canSwitchInfo = false;

    @Input()
    public parking: Parking;

    @HostListener('click')
    public onSwitchInfo() {
        if (!this.canSwitchInfo) {
            return;
        }

        this.selectedInfo = this.selectedInfo === InfoType.Started
            ? InfoType.Completed
            : InfoType.Started;
    }

    public ngOnInit() {
        switch (this.parking.type) {
            case ParkingType.StartedFree:
                this.selectedInfo = InfoType.Started;
                break;
            case ParkingType.CompletedFirst:
            case ParkingType.CompletedFree:
                this.selectedInfo = InfoType.Completed;
                this.canSwitchInfo = true;
                break;
            default:
                assertUnhandledType(this.parking);
        }
    }
}
