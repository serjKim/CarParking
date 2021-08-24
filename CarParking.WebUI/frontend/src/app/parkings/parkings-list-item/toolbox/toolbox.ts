import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { CompletionResultType, Parking, ParkingType } from '../../models';
import { ParkingsStorage } from '../../parkings.storage';

export enum ToolboxButtonType {
    Complete,
    Pay,
}

export type ToolBoxClick = (parking: Parking, toolbox: Toolbox) => Promise<void>;

const BUTTON_STATE = Symbol('Button mutable state.');

export class ToolboxButton {
    public [BUTTON_STATE] = {
        disabled: new BehaviorSubject(false),
        displayed: new BehaviorSubject(false),
    };
    public get disabled(): Observable<boolean> {
        return this[BUTTON_STATE].disabled;
    }
    public get displayed(): Observable<boolean> {
        return this[BUTTON_STATE].displayed;
    }
    constructor(
        public readonly type: ToolboxButtonType,
        public readonly onClick: ToolBoxClick,
    ) {}
}

export class Toolbox {
    constructor(
        public readonly buttons: ToolboxButton[],
    ) {}

    public displayPayButton() {
        for (const button of this.buttons) {
            button[BUTTON_STATE].displayed.next(button.type === ToolboxButtonType.Pay);
        }
    }
}

type ToolboxButtonSettings = { [key in ParkingType]: (completeButton: ToolboxButton, payButton: ToolboxButton) => void };

@Injectable()
export class ToolboxFactory {
    private readonly settingsByType: ToolboxButtonSettings = {
        [ParkingType.StartedFree]: (completeButton: ToolboxButton, payButton: ToolboxButton) => {
            completeButton[BUTTON_STATE].displayed.next(true);
            completeButton[BUTTON_STATE].disabled.next(false);
            payButton[BUTTON_STATE].displayed.next(false);
        },
        [ParkingType.CompletedFree]: (completeButton: ToolboxButton, payButton: ToolboxButton) => {
            completeButton[BUTTON_STATE].displayed.next(true);
            completeButton[BUTTON_STATE].disabled.next(true);
            payButton[BUTTON_STATE].displayed.next(false);
        },
        [ParkingType.CompletedFirst]: (completeButton: ToolboxButton, payButton: ToolboxButton) => {
            completeButton[BUTTON_STATE].displayed.next(false);
            payButton[BUTTON_STATE].displayed.next(true);
            payButton[BUTTON_STATE].disabled.next(true);
        },
    };

    constructor(
        private readonly parkingsStorage: ParkingsStorage,
    ) {}

    public createToolbox(type: ParkingType): Toolbox {
        const setupButtons = this.settingsByType[type];
        if (!setupButtons) {
            throw new Error(`Unexpected ${type} parking type.`);
        }

        const completeButton = new ToolboxButton(ToolboxButtonType.Complete, this.onComplete());
        const payButton = new ToolboxButton(ToolboxButtonType.Pay, this.onPay());

        setupButtons(completeButton, payButton);

        return new Toolbox([completeButton, payButton]);
    }

    private onComplete(): ToolBoxClick {
        return async (parking: Parking, toolbox: Toolbox) => {
            if (parking.type === ParkingType.StartedFree) {
                const result = await this.parkingsStorage.completeParking(parking);
                if (result.type === CompletionResultType.FreeExpired) {
                    toolbox.displayPayButton();
                }
            }
        };
    }

    private onPay(): ToolBoxClick {
        return (parking: Parking) => {
            return this.parkingsStorage.payParking(parking);
        };
    }
}
