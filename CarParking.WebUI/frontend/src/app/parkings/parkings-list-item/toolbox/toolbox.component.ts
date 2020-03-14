import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { assertUnhandledType } from 'src/app/util';
import { CompletionResultType } from '../../models/completion';
import { Parking, ParkingType } from '../../models/parking';
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
    public parking: Parking;

    public readonly toolboxButtonType = ToolboxButton;
    public selectedButton = ToolboxButton.Complete;
    public canComplete = false;
    public canPay = false;

    constructor() { }

    public async onComplete(e: ToolboxButtonEvent) {
        switch (e.eventType) {
            case ToolboxButtonEventType.Completion:
                if (e.completionResult === CompletionResultType.FreeExpired) {
                    this.selectedButton = ToolboxButton.Pay;
                }
                break;
            default:
                assertUnhandledType(e.eventType);
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
                assertUnhandledType(this.parking);
        }
    }
}
