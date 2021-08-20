import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { errorUnhandledType, notNullOrFail } from '../../../util';
import { CompletionResultType, Parking, ParkingType } from '../../models';
import { ToolboxButtonEvent, ToolboxButtonEventType } from './toolbox-button-event';

enum ToolboxButton {
    Complete,
    Pay,
}

@Component({
    selector: 'toolbox',
    templateUrl: './toolbox.component.html',
    styleUrls: ['./toolbox.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToolboxComponent implements OnInit {
    @Input()
    public set parking(val: Parking) {
        this.currentParking = val;
    }

    public get parking(): Parking {
        return notNullOrFail(this.currentParking);
    }

    public readonly toolboxButtonType = ToolboxButton;
    public selectedButton = ToolboxButton.Complete;
    public canComplete = false;
    public canPay = false;

    private currentParking: Parking | null = null;

    constructor() { }

    public async onComplete(e: ToolboxButtonEvent) {
        switch (e.eventType) {
            case ToolboxButtonEventType.Completion:
                if (e.completionResult === CompletionResultType.FreeExpired) {
                    this.selectedButton = ToolboxButton.Pay;
                }
                break;
            default:
                throw errorUnhandledType(e.eventType);
        }
    }

    public ngOnInit() {
        switch (this.parking.type) {
            case ParkingType.StartedFree:
                this.canComplete = true;
                this.canPay = true;
                break;
            case ParkingType.CompletedFree:
                this.canComplete = false;
                break;
            case ParkingType.CompletedFirst:
                this.canPay = false;
                this.selectedButton = ToolboxButton.Pay;
                break;
            default:
                throw errorUnhandledType(this.parking);
        }
    }
}
